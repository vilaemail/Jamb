using System;
using System.Threading.Tasks;
using Jamb.Communication.WireProtocol;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;
using Jamb.Common;

namespace Jamb.Communication
{
    class MessagePasser : IDisposable
    {
        private const int c_maxMessageSize = 1024 * 10; // in B
        private const int c_backoffTime = 500; // in ms
        private const int c_protocolVersion = 1;
        private NetworkStream m_stream;

        /// <summary>
        /// Construct MessagePasser with the given stream.
        /// Create instances through static factory methods.
        /// </summary>
        private MessagePasser(NetworkStream stream)
        {
            Debug.Assert(stream != null);
        }

        /// <summary>
        /// Creates a TcpListener and waits for a connection. First connecting client is used to construct MessagePasser.
        /// Throws TaskCanceledException on cancelation, otherwise CommunicationException.
        /// </summary>
        /// <param name="ip">Ip of the server (or selves) on which we will listen for connections.</param>
        /// <param name="port">Port on which we will listen to connections</param>
        /// <param name="cancelToken">Cancellation token for accepting socket</param>
        /// <returns>MessagePasser that can be used to communicate with the client.</returns>
        public static MessagePasser InstantiateForServer(IPAddress ip, int port, CancellationToken cancelToken)
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress ipAddress = new IPAddress(new byte[] { 192, 168, 1, 20 });

            TcpListener tcpListener = null;
            Socket socket = null;
            NetworkStream stream = null;
            try
            {
                // Listen and wait for a connection while enabling cancelation
                tcpListener = new TcpListener(ip, port);
                tcpListener.Start();
                socket = Task.Run(() => tcpListener.AcceptSocket(), cancelToken).WaitAndThrowActualException();
                // Create stream with the socket of connection
                stream = new NetworkStream(socket, true);
                // Message passer becomes owner of a stream
                return new MessagePasser(stream);
            }
            catch (Exception e)
            {
                // Dispose of eventual resources that were left behind
                if(stream != null)
                {
                    stream.Dispose();
                }
                else
                {
                    socket?.Dispose();
                }
                
                // Rethrow if canceled
                if(e is TaskCanceledException)
                {
                    throw;
                }
                // Throw a familiar exception
                throw new UnknownCommunicationException("Exception occured while trying to establish a connection as a server", e);
            }
            finally
            {
                // Stop listening
                tcpListener?.Stop();
            }
        }

        /// <summary>
        /// Tries to connect to the given server. If successful creates MessagePasser that can be used to communicate with the server.
        /// Throws TaskCanceledException on cancelation, otherwise UnknownCommunicationException. 
        /// </summary>
        /// <param name="ip">Ip address of the server to which we will try to connect.</param>
        /// <param name="port">Port on the server we should target</param>
        /// <param name="cancelToken">Cancellation token used when trying to connect.</param>
        /// <returns>MessagePasser that can be used to communicate with the server.</returns>
        public static MessagePasser InstantiateForClient(IPAddress ip, int port, CancellationToken cancelToken)
        {
            Socket socket = null;
            NetworkStream stream = null;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Task.Run(() => socket.Connect(ip, port), cancelToken).WaitAndThrowActualException();
                stream = new NetworkStream(socket, true);
                return new MessagePasser(stream);
            }
            catch (Exception e)
            {
                // Dispose of eventual resources that were left behind
                if (stream != null)
                {
                    stream.Dispose();
                }
                else
                {
                    socket?.Dispose();
                }
                
                // Rethrow if canceled
                if (e is TaskCanceledException)
                {
                    throw;
                }
                // Throw a familiar exception
                throw new UnknownCommunicationException("Exception occured while establish a connection as a client", e);
            }
        }

        /// <summary>
        /// Synchronously tries to send a message.
        /// Throws TaskCanceledException on cancelation, otherwise CommunicationException.
        /// </summary>
        public void SendMessage(Message messageToSend, CancellationToken cancelToken)
        {
            try
            {
                // Make serializer
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Message));//TODO: do we need known types
                byte[] dataToBeSent;
                // Serialize data
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    serializer.WriteObject(memoryStream, messageToSend);
                    dataToBeSent = memoryStream.ToArray();
                }
                // Send the data
                SendBytes(dataToBeSent);
            }
            catch(TaskCanceledException)
            {
                // We were canceled
                throw;
            }
            catch (Exception e) when (!(e is CommunicationException))
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
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Message));//TODO: do we need known types
                Message receivedMessage;
                // Deserialize message
                using (MemoryStream memoryStream = new MemoryStream(receivedData))
                {
                    receivedMessage = (Message)serializer.ReadObject(memoryStream);
                }
                // Return the resulting object
                return receivedMessage;
            }
            catch(TaskCanceledException)
            {
                // We were canceled
                throw;
            }
            catch(Exception e) when (!(e is CommunicationException))
            {
                throw new UnknownCommunicationException("Exception occured while trying to receive message", e);
            }
        }

        /// <summary>
        /// Sends provided bytes over the stream.
        /// </summary>
        private void SendBytes(byte[] bytesToSend)
        {
            Debug.Assert(bytesToSend != null);
            // Make sure message is the maximum acceptable
            if (bytesToSend.Length + 4 > c_maxMessageSize)
            {
                throw new ProtocolException("Size of message to be sent is larger than the maximum.");
            }
            // Get first 4 bytes of the message
            byte[] messageHeader = new byte[4];
            messageHeader[0] = (byte)(bytesToSend.Length / (256 * 256 * 256));
            messageHeader[1] = (byte)((bytesToSend.Length / (256 * 256)) % 256);
            messageHeader[2] = (byte)((bytesToSend.Length / (256)) % 256);
            messageHeader[3] = (byte)((bytesToSend.Length / (1)) % 256);
            // Send the bytes
            m_stream.Write(messageHeader, 0, messageHeader.Length);
            m_stream.Write(bytesToSend, 0, bytesToSend.Length);
        }

        /// <summary>
        /// Receives bytes from the stream
        /// </summary>
        private byte[] ReceiveBytes(CancellationToken cancelToken)
        {
            // Read the message header
            byte[] messageHeader = ReadBytesFromStream(4, cancelToken);
            // Extract message size from the header
            int messageSize = messageHeader[0] * 256 * 256 * 256 + messageHeader[1] * 256 * 256 + messageHeader[2] * 256 + messageHeader[3];
            // Make sure message size is acceptable
            if (messageSize > c_maxMessageSize)
            {
                throw new ProtocolException("Message to be received is too large.");
            }
            // Receive the message
            byte[] message = ReadBytesFromStream(messageSize, cancelToken);
            if (cancelToken.IsCancellationRequested) return null;
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
            if(cancelToken.IsCancellationRequested)
            {
                throw new TaskCanceledException("Cancelation request detected in ReadBytesFromStream.");
            }
            // Return received data
            return buffer;
        }

        public void Dispose()
        {
            m_stream?.Dispose();
        }
    }
}
