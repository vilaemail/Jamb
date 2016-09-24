using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Logging
{
	[Serializable]
	public class LogFileCreationException : Exception
	{
		internal LogFileCreationException()
		{
		}

		internal LogFileCreationException(string message)
			: base(message)
		{
		}

		internal LogFileCreationException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
