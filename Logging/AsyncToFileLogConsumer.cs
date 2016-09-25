using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Jamb.Logging
{
	internal class AsyncToFileLogConsumer : ILogConsumer
	{
		private readonly string m_logDirectory;
		private static readonly int c_logPeriod = 10000;

		public AsyncToFileLogConsumer(string logDirectory)
		{
			m_logDirectory = logDirectory;
		}

		private StreamWriter m_logFile;
		private ConcurrentQueue<string> m_messageQueue;
		private Thread m_logThread;
		private bool m_stopThread = false;

		public void Initialize()
		{
			CreateLogFile();
			m_messageQueue = new ConcurrentQueue<string>();
			m_logThread = new Thread(LogThreadFunc);
			m_logThread.Start();
		}

		public void AddLogEntry(string entry)
		{
			m_messageQueue.Enqueue(entry);
		}

		private void LogThreadFunc()
		{
			m_logFile.WriteLine("#Begining log file {0:O}#", DateTime.UtcNow);
			m_logFile.Flush();

			while (!m_stopThread)
			{
				try
				{
					Thread.Sleep(c_logPeriod);
				}
				catch (ThreadInterruptedException)
				{
					// Ignore, we will check in the while condition should we stop
				}
				LogMessagesFromQueue();
				m_logFile.Flush();
			}
		}

		private void LogMessagesFromQueue()
		{
			string toLog;
			while (m_messageQueue.TryDequeue(out toLog))
			{
				m_logFile.WriteLine(toLog);
			}
		}

		public void Dispose()
		{
			// Stop the logging thread
			m_stopThread = true;
			m_logThread?.Interrupt();
			m_logThread?.Join();
			m_logThread = null;
			// Finish the log file and close it
			m_logFile?.WriteLine("#EOF:" + m_messageQueue.Count + "#");
			m_logFile?.Flush();
			m_logFile?.Close();
			m_logFile?.Dispose();
			m_logFile = null;
		}

		private void CreateLogFile()
		{
			try
			{
				string folderFullPath = Path.GetFullPath(m_logDirectory).TrimEnd('\\');
				if (!Directory.Exists(m_logDirectory))
				{
					Directory.CreateDirectory(m_logDirectory);
				}

				DateTime now = DateTime.UtcNow;

				string logFilePath = string.Format("{0}\\{1:yyyy-MM-dd}_{1:H-mm-ss}.log", folderFullPath, now);
				if(File.Exists(logFilePath))
				{
					throw new LogFileCreationException("Log file already exists.");
				}

				m_logFile = new StreamWriter(logFilePath, false);
			}
			catch(Exception ex) when (ex is ArgumentException || ex is NotSupportedException)
			{
				throw new LogFileCreationException("Given path is invalid.", ex);
			}
			catch(UnauthorizedAccessException ex)
			{
				throw new LogFileCreationException("No privilidges to create log file on the given path.", ex);
			}
			catch(Exception ex) when (!(ex is LogFileCreationException))
			{
				throw new LogFileCreationException("Unexpected exception when creating log file.", ex);
			}
		}
	}
}
