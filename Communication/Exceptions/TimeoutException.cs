using System;

namespace Jamb.Communication
{
	[Serializable]
	public class TimeoutException : CommunicationException
	{
		internal TimeoutException()
		{
		}

		internal TimeoutException(string message)
			: base(message)
		{
		}

		internal TimeoutException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
