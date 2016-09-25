using Jamb.Common;
using System;
using System.Diagnostics;

namespace Jamb.Logging
{
	/// <summary>
	/// Determines if we should log based on minimum level. Ensures we don't try to log while logging. The inner logs are dropped.
	/// Valuidates arguments and computes the log data, handling any exception while computing.
	/// Sends the validated arguments to given instance of log formatter and its output is passed to the given instance of log consumer.
	/// </summary>
	internal class DefaultLogger : ILogger
	{
		private LogLevel m_minimumLogLevel;
		private readonly ILogConsumer m_logConsumer;
		private readonly ILogFormatter m_formatter;
		/// <summary>
		/// Whether or not we are in the process of logging a message on the given thread. If this is true, call to log will be ignored.
		/// </summary>
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

		/// <summary>
		/// Validates arguments. Makes sure we should log the message based on the log level.
		/// Logs while making sure we don't log during the log operation. 
		/// </summary>
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

		/// <summary>
		/// Validates arguments. Makes sure we should log the message based on the log level.
		/// Logs while making sure we don't log during the log operation. 
		/// </summary>
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

		/// <summary>
		/// Validates arguments. Makes sure we should log the message based on the log level.
		/// Logs while making sure we don't log during the log operation. 
		/// </summary>
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

		/// <summary>
		/// Disposes of all resources.
		/// </summary>
		public void Dispose()
		{
			m_logConsumer.Dispose();
		}

		/// <summary>
		/// Adds the log entry to the given consumer.
		/// </summary>
		private void AddLogEntry(string entry)
		{
			m_logConsumer.AddLogEntry(entry);
		}

		/// <summary>
		/// Makes sure no log calls are allowed while executing the provided actions
		/// </summary>
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

		/// <summary>
		/// Whether or not we should log the given message based on its log level.
		/// </summary>
		private bool ShouldLog(LogLevel level)
		{
			return level >= m_minimumLogLevel;
		}

		/// <summary>
		/// Tries to invoke given function and compute log data. Computed data is returned.
		/// If invocation fails it creates log data describing the exception while creating the data.
		/// </summary>
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
