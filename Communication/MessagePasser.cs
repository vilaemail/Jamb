using System;
using System.Threading.Tasks;
using Jamb.Communication.WireProtocol;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;

namespace Jamb.Communication
{
	/// <summary>
	/// Able to send and receive messages synchronosuly with another instance of this class on another computer.
	/// It is the owner of all resources that are used for this purpose.
	/// </summary>
	class MessagePasser : IDisposable
	{
		private const int c_maxMessageSize = 1024 * 10; // in B
		private const int c_backoffTime = 500; // in ms
		private const int c_headerSize = 5; // in B
		private const byte c_protocolVersion = 1;
		private const string c_readTimeoutMessage = "A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond";
		// Network stream we use to communicate.
		private readonly INetworkStream m_stream;

		/// <summary>
		/// Construct MessagePasser with the given stream.
		/// Create instances through approapriate factory.
		/// Ideally we would enclose this class and factory within same assembly, however that seems to be an overkill at the moment.
		/// </summary>
		internal MessagePasser(INetworkStream stream)
		{
			Debug.Assert(stream != null);

			m_stream = stream;
		}

		/// <summary>
		/// Synchronously tries to send a message.
		/// Throws TaskCanceledException on cancelation, otherwise CommunicationException.
		/// </summary>
		public void SendMessage(Message messageToSend, CancellationToken cancelToken)
		{
			if (messageToSend == null)
			{
				throw new ArgumentNullException(nameof(messageToSend));
			}

			try
			{
				// Make serializer
				DataContractJsonSerializer serializer = CreateSerializer();
				byte[] dataToBeSent;
				// Serialize data
				using (MemoryStream memoryStream = new MemoryStream())
				{
					serializer.WriteObject(memoryStream, messageToSend);
					dataToBeSent = memoryStream.ToArray();
				}
				// Send the data
				SendBytes(dataToBeSent, cancelToken);
			}
			catch (Exception e) when (!(e is CommunicationException || e is TaskCanceledException)) // Only wrap unexpected exceptions
			{
				throw new UnknownCommunicationException("Exception occured while trying to send a message", e);
			}
		}

		/// <summary>
		/// Synchronously tries to receive a message.
		/// Throws TaskCanceledException on cancelation, otherwise CommunicationException.
		/// </summary>
		public Message ReceiveMessage(CancellationToken cancelToken)
		{
			try
			{
				// Get bytes from the stream
				byte[] receivedData = ReceiveBytes(cancelToken);
				// Make deserializer
				DataContractJsonSerializer serializer = CreateSerializer();
				Message receivedMessage;
				// Deserialize message
				using (MemoryStream memoryStream = new MemoryStream(receivedData))
				{
					receivedMessage = (Message)serializer.ReadObject(memoryStream);
				}
				// Return the resulting object
				return receivedMessage;
			}
			catch (IOException e) when (e.InnerException != null && e.InnerException.GetType() == typeof(SocketException) && e.InnerException.Message.Contains(c_readTimeoutMessage))
			{
				// Read timeout
				throw new TimeoutException("Timedout during read operation", e);
			}
			catch (SerializationException e)
			{
				throw new MalformedMessageException("Failed to deserialize received message", e);
			}
			catch (Exception e) when (!(e is CommunicationException || e is TaskCanceledException)) // Only wrap unexpected exceptions
			{
				throw new UnknownCommunicationException("Exception occured while trying to receive message", e);
			}
		}

		/// <summary>
		/// Sends provided bytes over the stream.
		/// </summary>
		private void SendBytes(byte[] bytesToSend, CancellationToken cancelToken)
		{
			Debug.Assert(bytesToSend != null);
			// Make sure message is the maximum acceptable
			if (bytesToSend.Length + c_headerSize > c_maxMessageSize)
			{
				throw new ProtocolException("Size of message to be sent is larger than the maximum.");
			}
			// Construct the message header
			Debug.Assert(c_headerSize == 5);
			byte[] messageHeader = new byte[c_headerSize];
			messageHeader[0] = (byte)(bytesToSend.Length / (256 * 256 * 256));
			messageHeader[1] = (byte)((bytesToSend.Length / (256 * 256)) % 256);
			messageHeader[2] = (byte)((bytesToSend.Length / (256)) % 256);
			messageHeader[3] = (byte)((bytesToSend.Length / (1)) % 256);
			messageHeader[4] = c_protocolVersion;
			// Send the bytes
			m_stream.Write(messageHeader, 0, messageHeader.Length);
			ThrowIfWeShouldCancel(cancelToken, "Cancelation request detected in SendBytes.");
			m_stream.Write(bytesToSend, 0, bytesToSend.Length);
		}

		/// <summary>
		/// Receives bytes from the stream
		/// </summary>
		private byte[] ReceiveBytes(CancellationToken cancelToken)
		{
			// Read the message header
			byte[] messageHeader = ReadBytesFromStream(c_headerSize, cancelToken);
			// Extract message size and protocol version from the header
			Debug.Assert(c_headerSize == 5);
			int messageSize = messageHeader[0] * 256 * 256 * 256 + messageHeader[1] * 256 * 256 + messageHeader[2] * 256 + messageHeader[3];
			byte protocolVersion = messageHeader[4];
			// Make sure we support the protocol
			if (protocolVersion != c_protocolVersion)
			{
				throw new ProtocolException("Unsupported protocol version. Our version: " + c_protocolVersion + ". Received message version: " + protocolVersion);
			}
			// Make sure message size is acceptable
			if (messageSize > c_maxMessageSize)
			{
				throw new ProtocolException("Message to be received is too large.");
			}
			// Receive the message
			byte[] message = ReadBytesFromStream(messageSize, cancelToken);
			return message;
		}

		/// <summary>
		/// Reads the given number of bytes from the stream and returns them.
		/// If bytes are not available waits the specified backoff time and tries again.
		/// </summary>
		private byte[] ReadBytesFromStream(int numberOfBytes, CancellationToken cancelToken)
		{
			// Assert input argument
			Debug.Assert(numberOfBytes > 0);
			Debug.Assert(numberOfBytes <= c_maxMessageSize);
			// Prepare buffer for data that will be returned
			byte[] buffer = new byte[numberOfBytes];
			// Read from the stream
			int offset = 0;
			while (offset < numberOfBytes && !cancelToken.IsCancellationRequested)
			{
				// If there is no data backoff for some time
				if (!m_stream.DataAvailable)
				{
					Thread.Sleep(c_backoffTime);
					continue;
				}

				int justRead = m_stream.Read(buffer, offset, numberOfBytes - offset);
				offset += justRead;
			}
			// Test if we want to cancel
			ThrowIfWeShouldCancel(cancelToken, "Cancelation request detected in ReadBytesFromStream.");
			// Return received data
			return buffer;
		}

		/// <summary>
		/// Tests the token if cancelation is requested. If it is throws TaskCanceledException with the given message
		/// </summary>
		private static void ThrowIfWeShouldCancel(CancellationToken cancelToken, string exceptionMessage)
		{
			if (cancelToken.IsCancellationRequested)
			{
				throw new TaskCanceledException(exceptionMessage);
			}
		}

		private static DataContractJsonSerializer CreateSerializer()
		{
			return new DataContractJsonSerializer(typeof(Message), Message.KnownTypes);
		}

		public void Dispose()
		{
			m_stream?.Dispose();
		}
	}
}
