using System;
using System.Threading.Tasks;
using Jamb.Communication.WireProtocol;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;
using Jamb.Values;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Jamb.Communication
{
	/// <summary>
	/// 
	/// </summary>
	internal class Connection : IDisposable
	{
		private ConnectionState m_state = ConnectionState.Lost;
		private MessagePasser m_messagePasser = null;
		private Mutex m_sendMutex = new Mutex(false);
		private Mutex m_receiveMutex = new Mutex(false);

		internal ConnectionState State => m_state;

		/// <summary>
		/// 
		/// </summary>
		internal Connection()
		{
		}

		internal void MarkAsLost()
		{
			switch (m_state)
			{
				case ConnectionState.Open:
					m_state = ConnectionState.Lost;
					break;
				case ConnectionState.Lost:
					break;
				case ConnectionState.Closing:
					// Close since we lost connection there is no point in trying to send anything
					m_state = ConnectionState.Closed;
					break;
				case ConnectionState.Closed:
					break;
				default:
					throw new InvalidOperationException("Unexpected internal state: " + m_state.ToString());
			}

		}

		internal void MarkAsClosing()
		{
			switch(m_state)
			{
				case ConnectionState.Open:
					m_state = ConnectionState.Closing;
					break;
				case ConnectionState.Lost:
					// Close since we lost connection there is no point in trying to send anything
					m_state = ConnectionState.Closed;
					break;
				case ConnectionState.Closing:
				case ConnectionState.Closed:
					break;
				default:
					throw new InvalidOperationException("Unexpected internal state");
			}
		}

		internal void MarkAsClosed()
		{
			m_state = ConnectionState.Closed;
		}

		internal void Reopen(MessagePasser messagePasser)
		{
			using (new Releaser(m_sendMutex))
			using (new Releaser(m_receiveMutex))
			{ 
				if (m_state != ConnectionState.Lost)
				{
					throw new InvalidOperationException("Can't set message passer while not in a Lost state.");
				}

				m_messagePasser?.Dispose();

				m_messagePasser = messagePasser;
				m_state = ConnectionState.Open;
			}
		}

		internal void SendMessage(Message messageToSend, CancellationToken cancellationToken)
		{
			if(m_state == ConnectionState.Closed)
			{
				// stop sending now
			}
			if(m_state == ConnectionState.Lost)
			{
				// wait for state change and retest
			}
			using (new Releaser(m_sendMutex))
			{
				// send
				m_messagePasser.SendMessage(messageToSend, cancellationToken);
			}
		}

		internal Message ReceiveMessage(CancellationToken cancellationToken)
		{
			if (m_state == ConnectionState.Closed || m_state == ConnectionState.Closing)
			{
				// stop receiving now
			}
			if (m_state == ConnectionState.Lost)
			{
				// wait for state change and retest
			}
			using (new Releaser(m_receiveMutex))
			{
				// receive
				// Receive message with a timeout
				CancellationTokenSource messagePasserCancelationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				messagePasserCancelationTokenSource.CancelAfter(m_settings.SecondsSinceLastMessageForTimeout.Get() * 1000);
				return m_messagePasser.ReceiveMessage(messagePasserCancelationTokenSource.Token);
			}
		}

		private class Releaser : IDisposable
		{
			Mutex m_s;
			public Releaser(Mutex s)
			{
				m_s = s;
				m_s.WaitOne();
			}

			public void Dispose()
			{
				m_s.ReleaseMutex();
			}
		}

		public void Dispose()
		{
			m_messagePasser?.Dispose();
		}
	}
}
