using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Logging
{
	public interface ILogger : IDisposable
	{
		void Log(LogLevel level, string message);
		void Log(LogLevel level, string message, Exception e);
		void Log(LogLevel level, string message, Func<LogData> logDataCreator);
	}
}
