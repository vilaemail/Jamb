using System;

namespace Jamb.Communication
{
	[Serializable]
	public class MalformedMessageException : CommunicationException
	{
		internal MalformedMessageException()
		{
		}

		internal MalformedMessageException(string message)
			: base(message)
		{
		}

		internal MalformedMessageException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
