using Jamb.Common;
using Jamb.Communication;
using Jamb.Communication.WireProtocol;
using Jamb.Logging;
using JambTests.Assertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JambTests.Logging
{
	/// <summary>
	/// 
	/// 
	/// NOTE: These tests use actual file system!
	/// </summary>
	[TestClass]
	public class IntegrationTestAsyncToFileLogConsumer
	{
		[TestCleanup]
		public void Cleanup()
		{
			if (Directory.Exists("testlogs"))
			{
				Directory.Delete("testlogs", true);
			}
		}

		[TestCategory("Integration"), TestMethod]
		public void Initialize_WhenInitializedLogger_DirectoryCreatedAndLoggingThreadPresent()
		{
			var underTest = new AsyncToFileLogConsumer("testlogs");

			int threadsBeforeInitializingLogger = Process.GetCurrentProcess().Threads.Count;
			underTest.Initialize();
			int threadsAfterInitializingLogger = Process.GetCurrentProcess().Threads.Count;
			underTest.Dispose();
			int threadsAfterDisposingLogger = Process.GetCurrentProcess().Threads.Count;

			Assert.AreEqual(threadsBeforeInitializingLogger + 1, threadsAfterInitializingLogger, "We should have 1 thread more after logger initialization.");
			Assert.AreEqual(threadsBeforeInitializingLogger, threadsAfterDisposingLogger, "Same number of threads should be after logger initialization and disposal.");
			Assert.IsTrue(Directory.Exists("testlogs"), "We should have a log folder present");
			var files = Directory.EnumerateFiles("testlogs");
			Assert.AreEqual(1, files.Count(), "We should have exactly one file in log directory");
		}

		[TestCategory("Integration"), TestMethod]
		public void Initialize_InvalidPath_ExceptionThrown()
		{
			using (var underTest = new AsyncToFileLogConsumer(":!?\\fawno01!-@z/"))
			{
				AssertHelper.AssertExceptionHappened(() => underTest.Initialize(), typeof(LogFileCreationException), "We should fail for invalid path", "Given path is invalid");
			}
		}

		[TestCategory("Integration"), TestMethod]
		public void Initialize_NoAccessToPath_ExceptionThrown()
		{
			var winFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows).TrimEnd('\\') + '\\';
			using (var underTest = new AsyncToFileLogConsumer(winFolder + "_accessDenied"))
			{
				AssertHelper.AssertExceptionHappened(() => underTest.Initialize(), typeof(LogFileCreationException), "We should fail for path where we have no access rights", "No privilidges");
			}
		}

		[TestCategory("Integration"), TestMethod]
		public void Initialize_WhenInitializedAndDisposed_FileContainsStartAndEndLog()
		{
			using (var underTest = new AsyncToFileLogConsumer("testlogs"))
			{
				underTest.Initialize();
			}
			
			var files = Directory.EnumerateFiles("testlogs");
			string fileContents = File.ReadAllText(files.First());
			Assert.IsTrue(fileContents.Contains("Begining log file"), "We should have begining log in the file");
			Assert.IsTrue(fileContents.Contains("#EOF"), "We should have ending log in the file");
		}

		[TestCategory("Integration"), TestMethod]
		public void AddLogEntry_WithoutInitialization_ThrowsException()
		{
			using (var underTest = new AsyncToFileLogConsumer("testlogs"))
			{
				AssertHelper.AssertExceptionHappened(() => underTest.AddLogEntry("log"), typeof(InvalidOperationException), "We should throw if we try to log without initializing the consumer");
			}
		}

		[TestCategory("Integration"), TestMethod]
		public void AddLogEntry_AfterLoggingAndDisposing_FileContainsTheLog()
		{
			using (var underTest = new AsyncToFileLogConsumer("testlogs"))
			{
				underTest.Initialize();
				underTest.AddLogEntry("LOG!");
				underTest.AddLogEntry("CooL");
			}

			var files = Directory.EnumerateFiles("testlogs");
			string fileContents = File.ReadAllText(files.First());
			Assert.IsTrue(fileContents.Contains("LOG!"), "We should have the logged message in the file 1");
			Assert.IsTrue(fileContents.Contains("CooL"), "We should have the logged message in the file 2");
		}

		[TestCategory("Integration"), TestMethod]
		public void AddLogEntry_AfterLogging_FileDoesntContainTheLogImmediately()
		{
			using (var underTest = new AsyncToFileLogConsumer("testlogs"))
			{
				underTest.Initialize();
				underTest.AddLogEntry("LOG!");
				underTest.AddLogEntry("CooL");

				string fileContents = null;
				var files = Directory.EnumerateFiles("testlogs");
				using (var fileStream = File.Open(files.First(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var streamReader = new StreamReader(fileStream, Encoding.Default))
				{
					fileContents = streamReader.ReadToEnd();
				}

				Assert.IsFalse(fileContents.Contains("LOG!"), "We should not have the logged message in the file 1");
				Assert.IsFalse(fileContents.Contains("CooL"), "We should not have the logged message in the file 2");
			}
		}

		[TestCategory("Integration"), TestCategory("Longrunning"), TestMethod]
		public void AddLogEntry_AfterLogging_FileContainsLogAfterLogPeriodHasPassed()
		{
			using (var underTest = new AsyncToFileLogConsumer("testlogs"))
			using (var logPeriodChange = new FieldChanger<int>(typeof(AsyncToFileLogConsumer).GetField("c_logPeriod", BindingFlags.Instance | BindingFlags.NonPublic), underTest, 200))
			{
				underTest.Initialize();
				underTest.AddLogEntry("LOG!");
				underTest.AddLogEntry("CooL");

				Thread.Sleep(600);

				string fileContents = null;
				var files = Directory.EnumerateFiles("testlogs");
				using (var fileStream = File.Open(files.First(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var streamReader = new StreamReader(fileStream, Encoding.Default))
				{
					fileContents = streamReader.ReadToEnd();
				}

				Assert.IsTrue(fileContents.Contains("LOG!"), "We should have the logged message in the file 1");
				Assert.IsTrue(fileContents.Contains("CooL"), "We should have the logged message in the file 2");
			}
		}

		private class FieldChanger<T> : IDisposable
		{
			private T m_oldValue;
			private FieldInfo m_fieldInfo;
			private object m_belongingObject = null;
			public FieldChanger(FieldInfo fieldInfo, object belongingObject, T newValue)
			{
				m_fieldInfo = fieldInfo;
				m_belongingObject = belongingObject;
				m_oldValue = (T)m_fieldInfo.GetValue(m_belongingObject);
				m_fieldInfo.SetValue(m_belongingObject, newValue);
			}

			public void Dispose()
			{
				m_fieldInfo.SetValue(m_belongingObject, m_oldValue);
			}
		}
	}
}
