using System;

namespace Jamb.Communication
{
	[Serializable]
	public class ConnectionStateException : CommunicationException
	{
		internal ConnectionStateException()
		{
		}

		internal ConnectionStateException(string message)
			: base(message)
		{
		}

		internal ConnectionStateException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
