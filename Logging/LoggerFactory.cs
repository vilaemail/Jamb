using System;

namespace Jamb.Logging
{
	public class LoggerFactory
	{
		public ILogger CreateAsyncToFile(LogLevel minimumLogLevel, string logFolder)
		{
			//TODO: Integration test
			if(logFolder == null)
			{
				throw new ArgumentNullException(nameof(logFolder));
			}

			var consumer = new AsyncToFileLogConsumer(logFolder);
			consumer.Initialize();
			var formatter = new DefaultLogFormatter();

			return Create(minimumLogLevel, consumer, formatter);
		}

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
	}
}
