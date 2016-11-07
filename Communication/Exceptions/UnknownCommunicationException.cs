using System;

namespace Jamb.Communication
{
	[Serializable]
	public class UnknownCommunicationException : CommunicationException
	{
		internal UnknownCommunicationException()
		{
		}

		internal UnknownCommunicationException(string message)
			: base(message)
		{
		}

		internal UnknownCommunicationException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
