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
	public class Communicator : IDisposable
	{
		private readonly CommunicatorSettings m_settings;
		private readonly Connection m_connection;

		private Thread m_sendThread;
		private Thread m_receiveThread;
		private CancellationTokenSource m_sendThreadCanceler;
		private CancellationTokenSource m_receiveThreadCanceler;

		private BlockingCollection<Message> m_messagesForSending = new BlockingCollection<Message>(new ConcurrentQueue<Message>());
		private Queue<Message> m_sentMessages; //TODO: expose
		private Queue<Message> m_receivedMessages; //TODO: expose

		public ConnectionState State => m_connection.State;
		
		/// <summary>
		/// 
		/// </summary>
		internal Communicator(CommunicatorSettings settings)
		{
			Debug.Assert(settings != null);

			m_settings = settings;

			m_connection = new Connection();

			m_sendThread = new Thread(SenderThread);
			m_receiveThread = new Thread(ReceiverThread);
			m_sendThreadCanceler = new CancellationTokenSource();
			m_receiveThreadCanceler = new CancellationTokenSource();

			m_sentMessages = new Queue<Message>(m_settings.PastMessagesToKeep.Get());
			m_receivedMessages = new Queue<Message>(m_settings.PastMessagesToKeep.Get());

			m_sendThread.Start();
			m_receiveThread.Start();
		}

		private void SenderThread()
		{
			CancellationToken myCancelationToken = m_sendThreadCanceler.Token;

			Message messageToSend = null;
			while (!myCancelationToken.IsCancellationRequested)
			{
				try
				{
					// Get message from queue to send
					if (messageToSend == null)
					{
						messageToSend = m_messagesForSending.Take(myCancelationToken);
					}

					// Send message (this could fail)
					m_connection.SendMessage(messageToSend, myCancelationToken);

					// Make sure we rememeber what we have sent
					AddPastMessageToCollection(m_sentMessages, messageToSend);
					messageToSend = null;
				}
				catch(OperationCanceledException) //TODO: Migrate to this exception
				{
					// We are being canceled
					break;
				}
				catch(CommunicationException)
				{
					// We failed to send.
					m_connection.MarkAsLost();
				}
			}
		}

		private void ReceiverThread()
		{
			CancellationToken myCancelationToken = m_receiveThreadCanceler.Token;
			while (!myCancelationToken.IsCancellationRequested)
			{
				Message receivedMessage = m_connection.ReceiveMessage(myCancelationToken);

				// TODO: Notify about received message

				// Make sure we remember what we have received
				AddPastMessageToCollection(m_receivedMessages, receivedMessage);
			}
		}

		private void AddPastMessageToCollection(Queue<Message> collection, Message message)
		{
			lock (collection)
			{
				if (collection.Count >= m_settings.PastMessagesToKeep.Get())
				{
					collection.Dequeue();
				}
				collection.Enqueue(message);
			}
		}

		public void Close()
		{
			// Inform threads we should communicate what we have and close up
			m_connection.MarkAsClosing();

			// Make sure we try to terminate sender after specific time
			m_sendThreadCanceler.CancelAfter(m_settings.SecondsToWaitClosingBeforeTerminating.Get() * 1000);
			// We don't want to receive anything, we are closing
			m_receiveThreadCanceler.Cancel();
			m_receiveThread?.Join();
			m_receiveThread = null;
			m_sendThread?.Join();
			m_sendThread = null;

			m_connection.MarkAsClosed();
		}

		public void Terminate()
		{
			// Inform threads they should stop communicating
			m_connection.MarkAsClosed();

			// Terminate threads
			m_sendThreadCanceler.Cancel();
			m_receiveThreadCanceler.Cancel();
			m_receiveThread?.Join();
			m_receiveThread = null;
			m_sendThread?.Join();
			m_sendThread = null;
		}

		public void Dispose()
		{
			Terminate();
			m_connection.Dispose();
		}
	}
}
