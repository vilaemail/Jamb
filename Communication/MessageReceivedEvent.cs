using System;

namespace Jamb.Communication
{
	public class MessageReceivedEventData
	{
		public Message ReceivedMessage { get; set; }

		internal MessageReceivedEventData()
		{
		}
	}
}
