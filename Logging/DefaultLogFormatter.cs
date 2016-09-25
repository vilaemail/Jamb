using Jamb.Common;
using System;
using System.Linq;

namespace Jamb.Logging
{
	/// <summary>
	/// Formats the log so that it is in the single line, contains time, log level and given information.
	/// </summary>
	internal class DefaultLogFormatter : ILogFormatter
	{
		/// <summary>
		/// Formats the log so that it is in the single line, contains time, log level and message.
		/// </summary>
		public string Format(LogLevel level, string message)
		{
			return string.Format("@Time: {0:O}; LogLevel: {1}; Message: {2}@", DateTime.UtcNow, level.ToString(), message.Replace("\n", "\\n").Replace("\r", "\\r"));
		}

		/// <summary>
		/// Formats the log so that it is in the single line, contains time, log level, message and data.
		/// </summary>
		public string Format(LogLevel level, string message, LogData logData)
		{
			return string.Format("@Time: {0:O}; LogLevel: {1}; Message: {2}; Data: {{{3}}}@", DateTime.UtcNow, level.ToString(), message, LogDataToString(logData)).Replace("\n", "\\n").Replace("\r", "\\r");
		}

		/// <summary>
		/// Formats the log so that it is in the single line, contains time, log level, message and exception information.
		/// </summary>
		public string Format(LogLevel level, string message, Exception e)
		{
			return string.Format("@Time: {0:O}; LogLevel: {1}; Message: {2}; Exception: {3}@", DateTime.UtcNow, level.ToString(), message, ExceptionHandling.CreateStringDescribingException(e)).Replace("\n", "\\n").Replace("\r", "\\r");
		}

		/// <summary>
		/// Converts log data to single line string for logging.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private string LogDataToString(LogData data)
		{
			return string.Join(";", data.Select(x => x.Key + "=" + (x.Value ?? "null")));
		}
	}
}
