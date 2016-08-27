using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Communication
{
	public class UnknownCommunicationException : CommunicationException
	{
		public UnknownCommunicationException()
		{
		}

		public UnknownCommunicationException(string message)
			: base(message)
		{
		}

		public UnknownCommunicationException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
