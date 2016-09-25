using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Logging
{
	/// <summary>
	/// Provides basic logging functionalities for the application.
	/// </summary>
	public interface ILogger : IDisposable
	{
		/// <summary>
		/// Log the message with given level.
		/// </summary>
		void Log(LogLevel level, string message);
		/// <summary>
		/// Log the message and exception with given level.
		/// </summary>
		void Log(LogLevel level, string message, Exception e);
		/// <summary>
		/// Log the message and data with given level.
		/// </summary>
		void Log(LogLevel level, string message, Func<LogData> logDataCreator);
	}
}
