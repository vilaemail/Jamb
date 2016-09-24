using Jamb.Common;
using System;
using System.Diagnostics;

namespace Jamb.Logging
{
	internal class DefaultLogger : ILogger
	{
		private LogLevel m_minimumLogLevel;
		private readonly ILogConsumer m_logConsumer;
		private readonly ILogFormatter m_formatter;
		[ThreadStatic]
		private static bool m_logInProgress = false;

		public DefaultLogger(LogLevel minimumLogLevel, ILogConsumer consumer, ILogFormatter formatter)
		{
			Debug.Assert(consumer != null);
			Debug.Assert(formatter != null);

			m_minimumLogLevel = minimumLogLevel;
			m_logConsumer = consumer;
			m_formatter = formatter;
		}

		public void Log(LogLevel level, string message)
		{
			EnsureNoLogs(() =>
			{
				if (message == null)
				{
					throw new ArgumentNullException(nameof(message));
				}

				if (!ShouldLog(level))
				{
					return;
				}

				AddLogEntry(m_formatter.Format(level, message));
			});
		}

		public void Log(LogLevel level, string message, Func<LogData> logDataCreator)
		{
			EnsureNoLogs(() =>
			{
				if (message == null)
				{
					throw new ArgumentNullException(nameof(message));
				}
				if (logDataCreator == null)
				{
					throw new ArgumentNullException(nameof(logDataCreator));
				}

				if (!ShouldLog(level))
				{
					return;
				}

				LogData logData = TryGetLogData(logDataCreator);

				AddLogEntry(m_formatter.Format(level, message, logData));
			});
		}

		public void Log(LogLevel level, string message, Exception e)
		{
			EnsureNoLogs(() =>
			{
				if (message == null)
				{
					throw new ArgumentNullException(nameof(message));
				}
				if (e == null)
				{
					throw new ArgumentNullException(nameof(e));
				}

				if (!ShouldLog(level))
				{
					return;
				}

				AddLogEntry(m_formatter.Format(level, message, e));
			});
		}

		public void Dispose()
		{
			m_logConsumer.Dispose();
		}

		private void AddLogEntry(string entry)
		{
			m_logConsumer.AddLogEntry(entry);
		}

		private void EnsureNoLogs(Action actionDuringWhichLogsAreIgnored)
		{
			if (m_logInProgress)
			{
				return;
			}

			try
			{
				m_logInProgress = true;
				actionDuringWhichLogsAreIgnored();
			}
			finally
			{
				m_logInProgress = false;
			}
		}

		private bool ShouldLog(LogLevel level)
		{
			return level >= m_minimumLogLevel;
		}

		private static LogData TryGetLogData(Func<LogData> logDataCreator)
		{
			LogData data = null;
			try
			{
				data = logDataCreator();
			}
			catch (Exception e)
			{
				data = new LogData() { { "TryGetLogDataException", ExceptionHandling.CreateStringDescribingException(e) } };
			}
			return data;
		}
	}
}
