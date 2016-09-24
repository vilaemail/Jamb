using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Logging
{
	public interface ILogFormatter
	{
		string Format(LogLevel level, string message);
		string Format(LogLevel level, string message, Exception e);
		string Format(LogLevel level, string message, LogData logData);
	}
}
