using Jamb.Common;
using Jamb.Communication.WireProtocol;
using System;
using System.Diagnostics;
using System.Threading;

namespace Jamb.Communication
{
	/// <summary>
	/// Owns message passer and its state. Changes the state if it detects a change.
	/// Connection can be reestablished with new IMessagePasser through Reopen method by ConnectionManager.
	/// </summary>
	internal class Connection : IDisposable
	{
		private readonly ITaskFactory m_taskFactory;

		private ConnectionState m_state = ConnectionState.Lost;
		private IMessagePasser m_messagePasser = null;
		private Mutex m_sendMutex = new Mutex(false);
		private Mutex m_receiveMutex = new Mutex(false);
		private ManualResetEventSlim m_sendConnectionReopenedEvent = new ManualResetEventSlim(false);
		private ManualResetEventSlim m_receiveConnectionReopenedEvent = new ManualResetEventSlim(false);

		internal ConnectionState State => m_state;

		internal Connection(ITaskFactory taskFactory)
		{
			Debug.Assert(taskFactory != null);

			m_taskFactory = taskFactory;
		}

		/// <summary>
		/// Changes the state of connection given the knowledge that connection is lost.
		/// </summary>
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

		/// <summary>
		/// Changes state to closed and disposes of underlying message passer.
		/// </summary>
		private void ChangeStateToClosed()
		{
			m_state = ConnectionState.Closed;
			m_taskFactory.StartNew(() =>
			{
				using (new Releaser(m_sendMutex))
				using (new Releaser(m_receiveMutex))
				{
					m_messagePasser?.Dispose();
					m_messagePasser = null;
				}
			});
		}

		/// <summary>
		/// Reopens a lost connection by disposing of current message passer and marking a connection as opened with a new one.
		/// </summary>
		internal void Reopen(IMessagePasser messagePasser)
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
			m_sendConnectionReopenedEvent.Set();
			m_receiveConnectionReopenedEvent.Set();
		}

		/// <summary>
		/// Once this method returns false it will always return false. The connection will never support sending again because it is closed.
		/// </summary>
		internal bool SupportsSending()
		{
			return m_state != ConnectionState.Closed;
		}

		/// <summary>
		/// Sends a message.
		/// Throws ConnectionStateException if we don't support sending.
		/// Blocks to wait a state change if connection is broken, but repairable.
		/// Blocks to send a message.
		/// Throws a OperationCanceledException if it timesout or operation is canceled.
		/// </summary>
		internal void SendMessage(Message messageToSend, CancellationToken cancellationToken)
		{
			// Loop until we send or throw
			while (true)
			{
				// Make sure we can send
				WaitUntilSendingIsPossible(cancellationToken);

				using (new Releaser(m_sendMutex))
				{
					// Once we have a lock make sure we can still send
					if (IsSendingPossible())
					{
						// send
						ExecuteAndMarkAsLostOnException(() => m_messagePasser.SendMessage(messageToSend, cancellationToken));
						return;
					}
				}
			}
		}

		/// <summary>
		/// Returns when sending is possible. Otherwise throws an exception.
		/// </summary>
		private void WaitUntilSendingIsPossible(CancellationToken token)
		{
			bool readyForSending = false;
			do
			{
				m_sendConnectionReopenedEvent.Reset();
				if (!SupportsSending())
				{
					throw new ConnectionStateException("Can't send on closed connection");
				}
				if (m_state == ConnectionState.Lost)
				{
					m_sendConnectionReopenedEvent.Wait(token);
					continue;
				}
				readyForSending = true;
			}
			while (!readyForSending);
		}

		/// <summary>
		/// Can we send at this moment?
		/// </summary>
		private bool IsSendingPossible()
		{
			return SupportsSending() && m_state != ConnectionState.Lost;
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
			// Loop until we receive or throw
			while (true)
			{
				// Make sure we can receive
				WaitUntilReceivingIsPossible(cancellationToken);

				using (new Releaser(m_receiveMutex))
				{
					// Once we have a lock make sure we can still receive
					if (IsReceivingPossible())
					{
						// receive
						return ExecuteAndMarkAsLostOnException(() => m_messagePasser.ReceiveMessage(cancellationToken));
					}
				}
			}
		}

		/// <summary>
		/// Returns when receiving is possible. Otherwise throws an exception.
		/// </summary>
		private void WaitUntilReceivingIsPossible(CancellationToken token)
		{
			bool readyForReceiving = false;
			do
			{
				m_receiveConnectionReopenedEvent.Reset();
				if (!SupportsReceiving())
				{
					throw new ConnectionStateException("Can't receive on connection that is closing or is closed");
				}

				if (m_state == ConnectionState.Lost)
				{
					m_receiveConnectionReopenedEvent.Wait(token);
					continue;
				}

				readyForReceiving = true;
			} while (!readyForReceiving);
		}

		/// <summary>
		/// Can we receive at this moment?
		/// </summary>
		private bool IsReceivingPossible()
		{
			return SupportsReceiving() && m_state != ConnectionState.Lost;
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
