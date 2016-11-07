using System;
using System.Net.Sockets;
using System.Threading;

namespace Jamb.Communication.Network
{
	/// <summary>
	/// Calls the underlying implementation of NetworkStream.
	/// </summary>
	internal class WrappedNetworkStream : INetworkStream
	{
		private NetworkStream m_networkStream;
		public WrappedNetworkStream(NetworkStream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			m_networkStream = stream;
		}

		public bool DataAvailable => m_networkStream.DataAvailable;
		
		public void Dispose()
		{
			m_networkStream.Dispose();
		}

		public int Read(byte[] buffer, int offset, int size)
		{
			return m_networkStream.Read(buffer, offset, size);
		}

		public void Write(byte[] buffer, int offset, int size, CancellationToken token)
		{
			m_networkStream.WriteAsync(buffer, offset, size).Wait(token);
			m_networkStream.Flush();
		}
	}
}
