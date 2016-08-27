using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Communication
{
	public class MalformedMessageException : CommunicationException
	{
		public MalformedMessageException()
		{
		}

		public MalformedMessageException(string message)
			: base(message)
		{
		}

		public MalformedMessageException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
