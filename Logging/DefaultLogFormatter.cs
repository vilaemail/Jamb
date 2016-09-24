using Jamb.Common;
using System;
using System.Linq;

namespace Jamb.Logging
{
	internal class DefaultLogFormatter : ILogFormatter
	{
		public string Format(LogLevel level, string message)
		{
			return string.Format("@Time: {0:O}; LogLevel: {1}; Message: {2}@", DateTime.UtcNow, level.ToString(), message.Replace("\n", "\\n").Replace("\r", "\\r"));
		}

		public string Format(LogLevel level, string message, LogData logData)
		{
			return string.Format("@Time: {0:O}; LogLevel: {1}; Message: {2}; Data: {{{3}}}@", DateTime.UtcNow, level.ToString(), message, LogDataToString(logData)).Replace("\n", "\\n").Replace("\r", "\\r");
		}

		public string Format(LogLevel level, string message, Exception e)
		{
			return string.Format("@Time: {0:O}; LogLevel: {1}; Message: {2}; Exception: {3}@", DateTime.UtcNow, level.ToString(), message, ExceptionHandling.CreateStringDescribingException(e)).Replace("\n", "\\n").Replace("\r", "\\r");
		}

		private string LogDataToString(LogData data)
		{
			return string.Join(";", data.Select(x => x.Key + "=" + (x.Value ?? "null")));
		}
	}
}
