using System;

namespace Jamb.Logging
{
	/// <summary>
	/// All calls do nothing.
	/// </summary>
	internal class NullLogger : ILogger
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
		/// Does nothing
		/// </summary>
		public void Log(LogLevel level, string message, Func<LogData> logDataCreator)
		{
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		public void Log(LogLevel level, string message, Exception e)
		{
		}
	}
}