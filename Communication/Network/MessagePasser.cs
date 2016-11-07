using Jamb.Communication.WireProtocol;
using Jamb.Values;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;

namespace Jamb.Communication.Network
{
	/// <summary>
	/// Able to send and receive messages synchronosuly with another instance of this class on another computer.
	/// It is the owner of all resources that are used for this purpose.
	/// </summary>
	internal class MessagePasser : IMessagePasser
	{
		private const int c_headerSize = 5; // in B
		private const byte c_protocolVersion = 1;
		private const string c_readTimeoutMessage = "A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond";
		// Network stream we use to communicate.
		private readonly INetworkStream m_stream;
		private readonly IValue<int> m_maxMessageSize; // in B
		private readonly IValue<int> m_backoffTime; // in ms

		/// <summary>
		/// Construct MessagePasser with the given stream.
		/// Create instances through approapriate factory.
		/// Ideally we would enclose this class and factory within same assembly, however that seems to be an overkill at the moment.
		/// </summary>
		internal MessagePasser(INetworkStream stream, IValue<int> maxMessageSizeInB, IValue<int> backoffInMs)
		{
			Debug.Assert(stream != null);
			Debug.Assert(maxMessageSizeInB != null);
			Debug.Assert(backoffInMs != null);

			m_stream = stream;
			m_maxMessageSize = maxMessageSizeInB;
			m_backoffTime = backoffInMs;
		}

		/// <summary>
		/// Synchronously tries to send a message.
		/// Throws OperationCanceledException on cancelation, otherwise CommunicationException.
		/// </summary>
		public void SendMessage(Message messageToSend, CancellationToken cancelToken)
		{
			if (messageToSend == null)
			{
				throw new ArgumentNullException(nameof(messageToSend));
			}

			try
			{
				byte[] dataToBeSent = SerializeMessage(messageToSend);
				SendBytes(dataToBeSent, cancelToken);
			}
			catch (Exception e) when (!(e is CommunicationException || e is OperationCanceledException)) // Only wrap unexpected exceptions
			{
				throw new UnknownCommunicationException("Exception occured while trying to send a message", e);
			}
		}

		/// <summary>
		/// Synchronously tries to receive a message.
		/// Throws OperationCanceledException on cancelation, otherwise CommunicationException.
		/// </summary>
		public Message ReceiveMessage(CancellationToken cancelToken)
		{
			try
			{
				// Get bytes from the stream
				byte[] receivedData = ReceiveBytes(cancelToken);
				return DeserializeMessage(receivedData);
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
			catch (Exception e) when (!(e is CommunicationException || e is OperationCanceledException)) // Only wrap unexpected exceptions
			{
				throw new UnknownCommunicationException("Exception occured while trying to receive message", e);
			}
		}

		/// <summary>
		/// Serializes given message to byte array.
		/// </summary>
		private static byte[] SerializeMessage(Message message)
		{
			DataContractJsonSerializer serializer = CreateSerializer();
			// Serialize message to byte array
			using (MemoryStream memoryStream = new MemoryStream())
			{
				serializer.WriteObject(memoryStream, message);
				return memoryStream.ToArray();
			}
		}

		/// <summary>
		/// Deserializes given byte array to message.
		/// </summary>
		private Message DeserializeMessage(byte[] data)
		{
			DataContractJsonSerializer serializer = CreateSerializer();
			// Deserialize message
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				return (Message)serializer.ReadObject(memoryStream);
			}
		}

		/// <summary>
		/// Sends provided bytes over the stream.
		/// </summary>
		private void SendBytes(byte[] bytesToSend, CancellationToken cancelToken)
		{
			Debug.Assert(bytesToSend != null);
			// Make sure message is the maximum acceptable
			if (bytesToSend.Length + c_headerSize > m_maxMessageSize.Get())
			{
				throw new ProtocolException("Size of message to be sent is larger than the maximum.");
			}

			byte[] messageHeader = ConstructMessageHeader(bytesToSend);
			// Send the bytes
			m_stream.Write(messageHeader, 0, messageHeader.Length, cancelToken);
			ThrowIfWeShouldCancel(cancelToken, "Cancelation request detected in SendBytes.");
			m_stream.Write(bytesToSend, 0, bytesToSend.Length, cancelToken);
		}

		/// <summary>
		/// Creates header to be sent before the actual payload
		/// </summary>
		private static byte[] ConstructMessageHeader(byte[] bytesToSend)
		{
			Debug.Assert(c_headerSize == 5);
			byte[] messageHeader = new byte[c_headerSize];
			messageHeader[0] = (byte)(bytesToSend.Length / (256 * 256 * 256));
			messageHeader[1] = (byte)((bytesToSend.Length / (256 * 256)) % 256);
			messageHeader[2] = (byte)((bytesToSend.Length / (256)) % 256);
			messageHeader[3] = (byte)((bytesToSend.Length / (1)) % 256);
			messageHeader[4] = c_protocolVersion;
			return messageHeader;
		}

		/// <summary>
		/// Receives bytes from the stream
		/// </summary>
		private byte[] ReceiveBytes(CancellationToken cancelToken)
		{
			// Receive header and process it
			byte[] messageHeader = ReadBytesFromStream(c_headerSize, cancelToken);
			int messageSize = UnpackAndValidateMessageHeader(messageHeader);
			if (messageSize > m_maxMessageSize.Get())
			{
				throw new ProtocolException("Message to be received is too large.");
			}
			// Receive the message (payload)
			byte[] message = ReadBytesFromStream(messageSize, cancelToken);
			return message;
		}

		/// <summary>
		/// Unpacks the header, validates it and returns the payload size.
		/// </summary>
		private static int UnpackAndValidateMessageHeader(byte[] messageHeader)
		{
			Debug.Assert(c_headerSize == 5);
			int messageSize = messageHeader[0] * 256 * 256 * 256 + messageHeader[1] * 256 * 256 + messageHeader[2] * 256 + messageHeader[3];

			byte protocolVersion = messageHeader[4];
			if (protocolVersion != c_protocolVersion)
			{
				throw new ProtocolException("Unsupported protocol version. Our version: " + c_protocolVersion + ". Received message version: " + protocolVersion);
			}

			return messageSize;
		}

		/// <summary>
		/// Reads the given number of bytes from the stream and returns them.
		/// If bytes are not available waits the specified backoff time and tries again.
		/// </summary>
		private byte[] ReadBytesFromStream(int numberOfBytes, CancellationToken cancelToken)
		{
			// Assert input argument
			Debug.Assert(numberOfBytes > 0);
			Debug.Assert(numberOfBytes <= m_maxMessageSize.Get());
			// Prepare buffer for data that will be returned
			byte[] buffer = new byte[numberOfBytes];
			// Read from the stream
			int offset = 0;
			while (offset < numberOfBytes)
			{
				// Test if we want to cancel
				ThrowIfWeShouldCancel(cancelToken, "Cancelation request detected in ReadBytesFromStream.");
				// If there is no data backoff for some time
				if (!m_stream.DataAvailable)
				{
					Thread.Sleep(m_backoffTime.Get());
					continue;
				}

				int justRead = m_stream.Read(buffer, offset, numberOfBytes - offset);
				offset += justRead;
			}
			
			// Return received data
			return buffer;
		}

		/// <summary>
		/// Tests the token if cancelation is requested. If it is throws OperationCanceledException with the given message
		/// </summary>
		private static void ThrowIfWeShouldCancel(CancellationToken cancelToken, string exceptionMessage)
		{
			if (cancelToken.IsCancellationRequested)
			{
				throw new OperationCanceledException(exceptionMessage);
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
