using System;

namespace Jamb.Logging
{
	/// <summary>
	/// Doesn't log anything. Evaluates LogData without exception handling.
	/// </summary>
	internal class NullLoggerThatEvaluatesLogData : ILogger
	{
		/// <summary>
		/// Does nothing
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		public void Log(LogLevel level, string message)
		{
		}

		/// <summary>
		/// Evaluates log data
		/// </summary>
		public void Log(LogLevel level, string message, Func<LogData> logDataCreator)
		{
			logDataCreator();
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		public void Log(LogLevel level, string message, Exception e)
		{
		}
	}
}