using Jamb.Values;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Jamb.Logging
{
	/// <summary>
	/// Consumes the given logs by writting them to file asynchronously on another thread.
	/// Needs to be initialized before it is used.
	/// </summary>
	internal class AsyncToFileLogConsumer : ILogConsumer
	{
		private readonly string m_logDirectory;
		private readonly string m_logFileNaming;
		private readonly IValue<int> m_logPeriod;
		private bool m_initialized = false;

		/// <summary>
		/// Creates a log consumer that will write to the given log directory.
		/// </summary>
		/// <param name="logDirectory">Where will we store our log files</param>
		/// <param name="logFileNaming">Template for naming schema of our files</param>
		/// <param name="logPeriodInS">After how many seconds should we write logs from memory to file</param>
		public AsyncToFileLogConsumer(string logDirectory, string logFileNaming, IValue<int> logPeriodInS)
		{
			Debug.Assert(!string.IsNullOrEmpty(logDirectory));
			Debug.Assert(!string.IsNullOrEmpty(logFileNaming));
			Debug.Assert(logPeriodInS != null);

			m_logDirectory = logDirectory;
			m_logFileNaming = logFileNaming;
			m_logPeriod = logPeriodInS;
		}

		private StreamWriter m_logFile;
		private ConcurrentQueue<string> m_messageQueue;
		private Thread m_logThread;
		private bool m_stopThread = false;

		/// <summary>
		/// Creates the log file and starts the asynchronous thread for writting to the file
		/// </summary>
		public void Initialize()
		{
			CreateLogFile();
			m_messageQueue = new ConcurrentQueue<string>();
			m_logThread = new Thread(LogThreadFunc);
			m_initialized = true;
			m_logThread.Start();
		}

		/// <summary>
		/// Consumes log entry by writting it to the queue.
		/// </summary>
		public void AddLogEntry(string entry)
		{
			if(!m_initialized)
			{
				throw new InvalidOperationException("Can't log when not initialized");
			}

			m_messageQueue.Enqueue(entry);
		}

		/// <summary>
		/// Executes asynchronously and periodically writes to file contents of the queue.
		/// </summary>
		private void LogThreadFunc()
		{
			Debug.Assert(m_initialized);

			m_logFile.WriteLine("#Begining log file {0:O}#", DateTime.UtcNow);
			m_logFile.Flush();

			while (!m_stopThread)
			{
				try
				{
					Thread.Sleep(m_logPeriod.Get());
				}
				catch (ThreadInterruptedException)
				{
					// Ignore, we will check in the while condition should we stop
				}
				LogMessagesFromQueue();
				m_logFile.Flush();
			}
		}

		/// <summary>
		/// Logs all messages from the queue to the file.
		/// </summary>
		private void LogMessagesFromQueue()
		{
			string toLog;
			while (m_messageQueue.TryDequeue(out toLog))
			{
				m_logFile.WriteLine(toLog);
			}
		}

		/// <summary>
		/// Releases all resources by stopping logging thread and finalizing the log file.
		/// </summary>
		public void Dispose()
		{
			// Stop the logging thread
			m_stopThread = true;
			m_logThread?.Interrupt();
			m_logThread?.Join();
			m_logThread = null;
			// Log if anything remained in our queue
			if (m_messageQueue != null && m_logFile != null)
			{
				LogMessagesFromQueue();
			}
			// Finish the log file and close it
			m_logFile?.WriteLine("#EOF:" + m_messageQueue.Count + "#");
			m_logFile?.Flush();
			m_logFile?.Close();
			m_logFile?.Dispose();
			m_logFile = null;
		}

		/// <summary>
		/// Creates the log file and throws LogFileCreationException if unsuccessful.
		/// </summary>
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

				string logFilePath = string.Format("{0}\\{1}", folderFullPath, string.Format(m_logFileNaming, now));
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
