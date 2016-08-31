using System;

namespace Jamb.Communication
{
	[Serializable]
	public class CommunicationException : Exception
	{
		internal CommunicationException()
		{
		}

		internal CommunicationException(string message)
			: base(message)
		{
		}

		internal CommunicationException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
