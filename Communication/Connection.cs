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
					ChangeStateToClosed();
					break;
				case ConnectionState.Closed:
					break;
				default:
					throw new InvalidOperationException("Unexpected internal state: " + m_state.ToString());
			}

		}

		/// <summary>
		/// Marks connection to be closed. Tries to send remaining messages.
		/// </summary>
		internal void BeginClosing()
		{
			switch(m_state)
			{
				case ConnectionState.Open:
					m_state = ConnectionState.Closing;
					break;
				case ConnectionState.Lost:
					// Close since we lost connection there is no point in trying to send anything
					ChangeStateToClosed();
					break;
				case ConnectionState.Closing:
				case ConnectionState.Closed:
					break;
				default:
					throw new InvalidOperationException("Unexpected internal state");
			}
		}

		/// <summary>
		/// Closes the connection without trying to send any more messages
		/// </summary>
		internal void Terminate()
		{
			ChangeStateToClosed();
		}

		private void ChangeStateToClosed()
		{
			m_state = ConnectionState.Closed;
			Task.Factory.StartNew(() =>
			{
				using (new Releaser(m_sendMutex))
				using (new Releaser(m_receiveMutex))
				{
					m_messagePasser?.Dispose();
					m_messagePasser = null;
				}
			});
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

		/// <summary>
		/// Once this method returns false it will always return false. The connection will never support sending again because it is closed.
		/// </summary>
		internal bool SupportsSending()
		{
			return m_state != ConnectionState.Closed;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="messageToSend"></param>
		/// <param name="cancellationToken"></param>
		internal void SendMessage(Message messageToSend, CancellationToken cancellationToken)
		{
			bool readyForSending = false;
			do
			{
				if (!SupportsSending())
				{
					throw new ConnectionStateException("Can't send on closed connection");
				}
				if (m_state == ConnectionState.Lost)
				{
					// wait for state change and retest
					readyForSending = true;
				}
			}
			while (!readyForSending);

			using (new Releaser(m_sendMutex))
			{
				// send
				ExecuteAndMarkAsLostOnException(() => m_messagePasser.SendMessage(messageToSend, cancellationToken));
			}
		}

		/// <summary>
		/// Once this method returns false it will always return false. The connection will never support receiving again because it is either closed or is in progress of being closed.
		/// </summary>
		internal bool SupportsReceiving()
		{
			return m_state != ConnectionState.Closed && m_state != ConnectionState.Closing;
		}

		/// <summary>
		/// Receives a message.
		/// Throws ConnectionStateException if we don't support receiving.
		/// Blocks to wait a state change if connection is broken, but repairable.
		/// Blocks to receive a message.
		/// Throws a OperationCanceledException if it timesout or operation is canceled.
		/// </summary>
		internal Message ReceiveMessage(CancellationToken cancellationToken, int timeoutInMs)
		{
			// Setup new cancelation token with given timeout
			CancellationTokenSource cancelationTokenSourceWithTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			cancelationTokenSourceWithTimeout.CancelAfter(timeoutInMs * 1000);

			return ReceiveMessage(cancelationTokenSourceWithTimeout.Token);
		}

		/// <summary>
		/// Receives a message.
		/// Throws ConnectionStateException if we don't support receiving.
		/// Blocks to wait a state change if connection is broken, but repairable.
		/// Blocks to receive a message.
		/// Throws a OperationCanceledException if operation is canceled.
		/// </summary>
		internal Message ReceiveMessage(CancellationToken cancellationToken)
		{
			bool readyForReceiving = false;
			do
			{
				if (!SupportsReceiving())
				{
					throw new ConnectionStateException("Can't receive on connection that is closing or is closed");
				}

				if (m_state == ConnectionState.Lost)
				{
					// wait for state change and retest
					continue;
				}

				readyForReceiving = true;
			} while (!readyForReceiving);

			using (new Releaser(m_receiveMutex))
			{
				return ExecuteAndMarkAsLostOnException(() => m_messagePasser.ReceiveMessage(cancellationToken));
			}
		}

		/// <summary>
		/// Executes given function. Any exception is caught, state is marked as Lost and exception is rethrown.
		/// </summary>
		private void ExecuteAndMarkAsLostOnException(Action func)
		{
			try
			{
				func();
			}
			catch(Exception)
			{
				MarkAsLost();
				throw;
			}
		}

		/// <summary>
		/// Executes given function. Any exception is caught, state is marked as Lost and exception is rethrown.
		/// If func is successful returns its return value.
		/// </summary>
		private T ExecuteAndMarkAsLostOnException<T>(Func<T> func)
		{
			try
			{
				return func();
			}
			catch (Exception)
			{
				MarkAsLost();
				throw;
			}
		}

		/// <summary>
		/// Helper class for acquiring and releasing a Mutex
		/// </summary>
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

		/// <summary>
		/// Disposes of underlying connection.
		/// </summary>
		public void Dispose()
		{
			m_messagePasser?.Dispose();
		}
	}
}
