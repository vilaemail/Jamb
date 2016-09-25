using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Logging
{
	/// <summary>
	/// Given the validated arguments formats them together to meaningfull log message represented by the string.
	/// </summary>
	public interface ILogFormatter
	{
		/// <summary>
		/// Create log message with given level and non-null message.
		/// </summary>
		string Format(LogLevel level, string message);

		/// <summary>
		/// Create log message with given level, non-null message and non-null exception.
		/// </summary>
		string Format(LogLevel level, string message, Exception e);

		/// <summary>
		/// Create log message with given level, non-null message and non-null data.
		/// </summary>
		string Format(LogLevel level, string message, LogData logData);
	}
}
