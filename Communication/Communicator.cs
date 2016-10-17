using Jamb.Communication.WireProtocol;
using Jamb.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Jamb.Communication
{
	/// <summary>
	/// Used for communication with another instance of this class on a remote computer.
	/// Exposes the state of underlying connection which is managed by Communication subsystem.
	/// </summary>
	public class Communicator : IDisposable
	{
		private readonly CommunicatorSettings m_settings;
		private readonly Connection m_connection;
		
		private Thread m_sendThread;
		private Thread m_receiveThread;
		private CancellationTokenSource m_sendThreadCanceler;
		private CancellationTokenSource m_receiveThreadCanceler;

		private BlockingCollection<Message> m_messagesForSending = new BlockingCollection<Message>(new ConcurrentQueue<Message>());
		private Queue<Message> m_sentMessages;
		private Queue<Message> m_receivedMessages;

		/// <summary>
		/// The state of underlying connection.
		/// </summary>
		public ConnectionState State => m_connection.State;

		/// <summary>
		/// Event fired when a message is received
		/// </summary>
		public event EventHandler<MessageReceivedEventData> MessageReceivedEvent;

		/// <summary>
		/// Constructs an instance with the given settings and connection. It owns the lifetime of connection.
		/// However the connection should be (re)established by ConnectionManager who is responsible for maintaining/making the connection functional.
		/// </summary>
		internal Communicator(CommunicatorSettings settings, Connection connection)
		{
			Debug.Assert(settings != null);
			Debug.Assert(connection != null);

			m_settings = settings;
			m_connection = connection;

			m_sendThread = new Thread(SenderThread);
			m_receiveThread = new Thread(ReceiverThread);
			m_sendThreadCanceler = new CancellationTokenSource();
			m_receiveThreadCanceler = new CancellationTokenSource();

			m_sentMessages = new Queue<Message>(m_settings.PastMessagesToKeep.Get());
			m_receivedMessages = new Queue<Message>(m_settings.PastMessagesToKeep.Get());

			m_sendThread.Start();
			m_receiveThread.Start();
		}

		/// <summary>
		/// Adds the given message to sending queue.
		/// </summary>
		public void QueueForSending(Message message)
		{
			m_messagesForSending.Add(message);
		}

		/// <summary>
		/// Returns a list of recently received messages in the receiving order.
		/// </summary>
		public IList<Message> ReceivedMessages()
		{
			lock(m_receivedMessages)
			{
				return m_receivedMessages.ToList();
			}
		}

		/// <summary>
		/// Returns a list of recently sent messages in the sending order.
		/// </summary>
		public IList<Message> SentMessages()
		{
			lock(m_sentMessages)
			{
				return m_sentMessages.ToList();
			}
		}

		/// <summary>
		/// This method is executed on a separate thread. Its job is to send messages and remember sent messages.
		/// </summary>
		private void SenderThread()
		{
			Debug.Assert(m_sendThreadCanceler != null);

			CancellationToken myCancelationToken = m_sendThreadCanceler.Token;
			Stopwatch timeSinceLastMessage = Stopwatch.StartNew();

			Message messageToSend = null;
			while (KeepSending(myCancelationToken))
			{
				try
				{
					if (messageToSend == null)
					{
						messageToSend = GetMessageToSend(myCancelationToken, timeSinceLastMessage.ElapsedMilliseconds);
					}

					// Send message (this could fail)
					m_connection.SendMessage(messageToSend, myCancelationToken);

					timeSinceLastMessage.Restart();
					AddPastMessageToCollection(m_sentMessages, messageToSend);
					messageToSend = null;
				}
				catch(OperationCanceledException e)
				{
					// We are being canceled or we timedout
					Logger.Instance.Log(LogLevel.Info, "OperationCanceledException when sending", e);
				}
				catch(CommunicationException e)
				{
					// We failed to send.
					Logger.Instance.Log(LogLevel.Warning, "CommunicationException when sending", e);
				}
			}
		}

		/// <summary>
		/// Gets the next message to be sent.
		/// If there is no message in queue and we haven't sent anything in a while a ping message is returned.
		/// If there is no message in queue and we have sent a message in the expected timeframe blocks to get a message from queue. Block is canceled when we need to send a ping message.
		/// </summary>
		private Message GetMessageToSend(CancellationToken token, long millisecondsSinceLastSentMessage)
		{
			Message messageToSend;

			if (m_messagesForSending.Any())
			{
				messageToSend = m_messagesForSending.Take(token);
			}
			else
			{
				// If we should have already sent a ping -> do so
				long timeout = m_settings.SecondsSinceLastSentMessageForPing.Get() * 1000 - millisecondsSinceLastSentMessage;
				if (timeout <= 0)
				{
					messageToSend = new PingMessage();
				}
				else
				{
					// If we shouldn't have sent a ping yet block for a new message
					CancellationTokenSource cancelationTokenSourceWithTimeout = CancellationTokenSource.CreateLinkedTokenSource(token);
					cancelationTokenSourceWithTimeout.CancelAfter((int)timeout);
					messageToSend = m_messagesForSending.Take(cancelationTokenSourceWithTimeout.Token);
				}
			}

			return messageToSend;
		}

		/// <summary>
		/// This method is executed on a separate thread. Its job is to receive messages, notify our subscribers and remember received messages.
		/// </summary>
		private void ReceiverThread()
		{
			Debug.Assert(m_receiveThreadCanceler != null);

			CancellationToken myCancelationToken = m_receiveThreadCanceler.Token;
			Stopwatch timeSinceLastMessage = Stopwatch.StartNew();

			while (KeepReceiving(myCancelationToken))
			{
				if(timeSinceLastMessage.ElapsedMilliseconds > m_settings.SecondsSinceLastMessageForTimeout.Get())
				{
					m_connection.MarkAsLost();
				}

				try
				{
					Message receivedMessage = m_connection.ReceiveMessage(myCancelationToken, m_settings.SecondsSinceLastMessageForTimeout.Get() * 1000);
					timeSinceLastMessage.Restart();

					// Notify about received message
					OnMessageReceived(receivedMessage);

					// Make sure we remember what we have received
					AddPastMessageToCollection(m_receivedMessages, receivedMessage);
				}
				catch (OperationCanceledException e) //TODO: Migrate to this exception
				{
					// We are being canceled or operation timedout
					Logger.Instance.Log(LogLevel.Info, "OperationCanceledException when receiving", e);
				}
				catch (CommunicationException e)
				{
					// We failed to receive
					Logger.Instance.Log(LogLevel.Warning, "CommunicationException when receiving", e);
				}
			}
		}

		/// <summary>
		/// Should we continue sending or stop the sender thread.
		/// </summary>
		private bool KeepSending(CancellationToken token)
		{
			Debug.Assert(token != null);
			Debug.Assert(m_connection != null);

			return !token.IsCancellationRequested && m_connection.SupportsSending();
		}

		/// <summary>
		/// Should we continue receiving or stop the receiver thread.
		/// </summary>
		private bool KeepReceiving(CancellationToken token)
		{
			Debug.Assert(token != null);
			Debug.Assert(m_connection != null);

			return !token.IsCancellationRequested && m_connection.SupportsReceiving();
		}

		/// <summary>
		/// Invokes all our subscribers for message received event.
		/// </summary>
		private void OnMessageReceived(Message message)
		{
			Debug.Assert(message != null);

			EventHandler<MessageReceivedEventData> handler = MessageReceivedEvent;
			if (handler != null)
			{
				var receptionEvent = new MessageReceivedEventData()
				{
					ReceivedMessage = message
				};
				handler(this, receptionEvent);
			}
		}

		/// <summary>
		/// Adds message to the given collection in a thread safe manner by locking collection.
		/// </summary>
		private void AddPastMessageToCollection(Queue<Message> collection, Message message)
		{
			Debug.Assert(collection != null);
			Debug.Assert(message != null);

			lock (collection)
			{
				if (collection.Count >= m_settings.PastMessagesToKeep.Get())
				{
					collection.Dequeue();
				}
				collection.Enqueue(message);
			}
		}

		/// <summary>
		/// Begins connection closing. Connection will be closed after all messages have been sent
		/// or too much time passes (as specified in settings).
		/// </summary>
		public void Close()
		{
			Debug.Assert(m_connection != null);

			// Inform threads we should communicate what we have and close up
			m_connection.BeginClosing();

			// Make sure we try to terminate sender after specific time
			m_sendThreadCanceler.CancelAfter(m_settings.SecondsToWaitClosingBeforeTerminating.Get() * 1000);
			// We don't want to receive anything, we are closing
			m_receiveThreadCanceler.Cancel();
			m_receiveThread?.Join();
			m_receiveThread = null;
			m_sendThread?.Join();
			m_sendThread = null;

			m_connection.Terminate();
		}

		/// <summary>
		/// Terminates the underlying connection. Messages for sending will not be sent.
		/// </summary>
		public void Terminate()
		{
			Debug.Assert(m_connection != null);

			// Inform threads they should stop communicating
			m_connection.Terminate();

			// Terminate threads
			m_sendThreadCanceler.Cancel();
			m_receiveThreadCanceler.Cancel();
			m_receiveThread?.Join();
			m_receiveThread = null;
			m_sendThread?.Join();
			m_sendThread = null;
		}

		/// <summary>
		/// Terminates underlying connection and disposes it.
		/// </summary>
		public void Dispose()
		{
			Debug.Assert(m_connection != null);

			Terminate();
			m_connection.Dispose();
		}
	}
}
