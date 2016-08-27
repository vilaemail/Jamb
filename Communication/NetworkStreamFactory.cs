using Jamb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jamb.Communication
{
	/// <summary>
	/// Creates NetworkStreams by using TcpListener, Socket, NetworkStream and WrappedNetworkStream.
	/// </summary>
	class NetworkStreamFactory
	{
		/// <summary>
		/// Creates a TcpListener and waits for a connection. First connecting client is used to construct MessagePasser.
		/// Throws TaskCanceledException on cancelation, otherwise CommunicationException.
		/// </summary>
		/// <param name="ip">Ip of the server (or selves) on which we will listen for connections.</param>
		/// <param name="port">Port on which we will listen to connections</param>
		/// <param name="cancelToken">Cancellation token for accepting socket</param>
		/// <returns>MessagePasser that can be used to communicate with the client.</returns>
		public INetworkStream InstantiateForServer(IPAddress ip, int port, CancellationToken cancelToken)
		{
			TcpListener tcpListener = null;
			Socket socket = null;
			NetworkStream stream = null;
			try
			{
				// Listen and wait for a connection while enabling cancelation
				tcpListener = new TcpListener(ip, port);
				tcpListener.Start();
				socket = Task.Run(() => tcpListener.AcceptSocket(), cancelToken).WaitAndThrowActualException();
				// Create stream with the socket of connection. Stream is owner of socket and connection
				stream = new NetworkStream(socket, true);
				// Set timeout to 10 seconds
				stream.ReadTimeout = 10000;
				stream.WriteTimeout = 10000;
				// Return a wrapper that is the owner of a stream
				return new WrappedNetworkStream(stream);
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
		public INetworkStream InstantiateForClient(IPAddress ip, int port, CancellationToken cancelToken)
		{
			Socket socket = null;
			NetworkStream stream = null;
			try
			{
				// Try to connect to the server
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				Task.Run(() => socket.Connect(ip, port), cancelToken).WaitAndThrowActualException();
				// Create stream with the socket of connection. Stream is owner of socket and connection
				stream = new NetworkStream(socket, true);
				// Return a wrapper that is the owner of a stream
				return new WrappedNetworkStream(stream);
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
				throw new UnknownCommunicationException("Exception occured while trying to establish a connection as a client", e);
			}
		}
	}
}
