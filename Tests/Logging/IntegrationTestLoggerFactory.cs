using Jamb.Logging;
using JambTests.Assertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace JambTests.Logging
{
	/// <summary>
	/// 
	/// 
	/// NOTE: These tests use actual file system!
	/// </summary>
	[TestClass]
	public class IntegrationTestLoggerFactory
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
		public void CreateAsyncToFile_NullLogFolder_ThrowsException()
		{
			var underTest = new LoggerFactory();

			AssertHelper.AssertExceptionHappened(() => underTest.CreateAsyncToFile(LogLevel.Info, null), typeof(ArgumentNullException), "We should throw for null log folder");
		}

		[TestCategory("Integration"), TestMethod]
		public void CreateAsyncToFile_ValidArguments_ExpectedTypesCreated()
		{
			var underTest = new LoggerFactory();

			ILogger logger = underTest.CreateAsyncToFile(LogLevel.Info, "testlogs");
			logger.Dispose();

			Assert.AreEqual(typeof(DefaultLogger), logger.GetType());
			Assert.IsTrue(Directory.Exists("testlogs"), "We should have a log directory created");
			var files = Directory.EnumerateFiles("testlogs");
			Assert.AreEqual(1, files.Count(), "We should have exactly one file in log directory");
		}
	}
}
