using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Logging
{
	/// <summary>
	/// Thrown when we fail to create log file.
	/// </summary>
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
