using System;

namespace Jamb.Communication
{
	[Serializable]
	public class ProtocolException : CommunicationException
	{
		internal ProtocolException()
		{
		}

		internal ProtocolException(string message)
			: base(message)
		{
		}

		internal ProtocolException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
