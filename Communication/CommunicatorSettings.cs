using Jamb.Values;
using System;

namespace Jamb.Communication
{
	public class CommunicatorSettings
	{
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

		internal IValue<int> PastMessagesToKeep { get; private set; }
		internal IValue<int> MessagesInQueueForTimeout { get; private set; }
		internal IValue<int> SecondsSinceLastMessageForTimeout { get; private set; }
		internal IValue<int> SecondsSinceLastSentMessageForPing { get; private set; }
		internal IValue<int> SecondsToWaitClosingBeforeTerminating { get; private set; }
	}
}
