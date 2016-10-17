using Jamb.Values;
using System;

namespace Jamb.Communication
{
	/// <summary>
	/// Holds the settings that customize the Communicator behavior
	/// </summary>
	public class CommunicatorSettings
	{
		/// <summary>
		/// Sets the settings for this instance.
		/// </summary>
		public CommunicatorSettings(IValue<int> pastMessagesToKeep,
			IValue<int> messagesInQueueForTimeout,
			IValue<int> secondsSinceLastMessageForTimeout,
			IValue<int> secondsSinceLastSentMessageForPing,
			IValue<int> secondsToWaitClosingBeforeTerminating)
		{
			if(pastMessagesToKeep == null)
			{
				throw new ArgumentNullException(nameof(pastMessagesToKeep));
			}
			if (messagesInQueueForTimeout == null)
			{
				throw new ArgumentNullException(nameof(messagesInQueueForTimeout));
			}
			if (secondsSinceLastMessageForTimeout == null)
			{
				throw new ArgumentNullException(nameof(secondsSinceLastMessageForTimeout));
			}
			if (secondsSinceLastSentMessageForPing == null)
			{
				throw new ArgumentNullException(nameof(secondsSinceLastSentMessageForPing));
			}
			if (secondsToWaitClosingBeforeTerminating == null)
			{
				throw new ArgumentNullException(nameof(secondsToWaitClosingBeforeTerminating));
			}

			PastMessagesToKeep = pastMessagesToKeep;
			MessagesInQueueForTimeout = messagesInQueueForTimeout;
			SecondsSinceLastMessageForTimeout = secondsSinceLastMessageForTimeout;
			SecondsSinceLastSentMessageForPing = secondsSinceLastSentMessageForPing;
			SecondsToWaitClosingBeforeTerminating = secondsToWaitClosingBeforeTerminating;
		}

		/// <summary>
		/// How much most recently sent messages and received messages should we keep.
		/// For example if this is set to 2 we will keep 2 last sent messages and 2 last received messages.
		/// </summary>
		internal IValue<int> PastMessagesToKeep { get; private set; }

		/// <summary>
		/// How much outstanding messages in queue will cause a connection to be marked as lost.
		/// </summary>
		internal IValue<int> MessagesInQueueForTimeout { get; private set; }

		/// <summary>
		/// If we don't receive a message for this amount of seconds we will mark connection as lost.
		/// </summary>
		internal IValue<int> SecondsSinceLastMessageForTimeout { get; private set; }

		/// <summary>
		/// If we don't send any application message in this timeframe a ping message will be sent.
		/// </summary>
		internal IValue<int> SecondsSinceLastSentMessageForPing { get; private set; }

		/// <summary>
		/// When connection gets in a state of closing, how long should we wait for outstanding messages to be sent before terminating the connection.
		/// </summary>
		internal IValue<int> SecondsToWaitClosingBeforeTerminating { get; private set; }
	}
}
