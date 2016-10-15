using Jamb.Values;
using System;

namespace Jamb.Logging
{
	/// <summary>
	/// Facade for creating loggers.
	/// </summary>
	public class LoggerFactory
	{
		/// <summary>
		/// Creates a default logger that logs to file in the provided folder.
		/// </summary>
		/// <param name="minimumLogLevel">All logs below this level will be ignored.</param>
		/// <param name="logFolder">Folder in which log file will reside.</param>
		/// <param name="logFileNaming">Template for naming our log files.</param>
		/// <param name="logPeriodInS">After how many seconds we should write logs to file.</param>
		/// <returns>Logger that can be used for logging.</returns>
		public ILogger CreateAsyncToFile(LogLevel minimumLogLevel, string logFolder, string logFileNaming, IValue<int> logPeriodInS)
		{
			if(logFolder == null)
			{
				throw new ArgumentNullException(nameof(logFolder));
			}
			if (logFileNaming == null)
			{
				throw new ArgumentNullException(nameof(logFileNaming));
			}
			if (logPeriodInS == null)
			{
				throw new ArgumentNullException(nameof(logPeriodInS));
			}

			var consumer = new AsyncToFileLogConsumer(logFolder, logFileNaming, logPeriodInS);
			consumer.Initialize();
			var formatter = new DefaultLogFormatter();

			return Create(minimumLogLevel, consumer, formatter);
		}

		/// <summary>
		/// Creates default logger with given consumer and formatter
		/// </summary>
		/// <param name="minimumLogLevel">All logs below this level will be ignored.</param>
		/// <param name="consumer">Instance that will receive strings that should be logged.</param>
		/// <param name="formatter">Instance that given the log contents will format them to a single string</param>
		/// <returns>Logger that can be used for logging.</returns>
		public ILogger Create(LogLevel minimumLogLevel, ILogConsumer consumer, ILogFormatter formatter)
		{
			if (consumer == null)
			{
				throw new ArgumentNullException(nameof(consumer));
			}
			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			return new DefaultLogger(minimumLogLevel, consumer, formatter);
		}

		/// <summary>
		/// Creates a logger that does nothing.
		/// </summary>
		public ILogger CreateNullLogger()
		{
			return new NullLogger();
		}
	}
}
