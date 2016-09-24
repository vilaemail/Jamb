using Jamb.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JambTests.Logging
{
	[TestClass]
	public class UnitTestDefaultLogFormatter
	{
		private const string c_testMessage = "test message\ntest\n\n\n!-?";
		private const string c_expectedFormattedMessage = "test message\\ntest\\n\\n\\n!-?";
		private const string c_exceptionMessage = "Exc\neption";
		private const string c_expectedFormattedExceptionMessage1 = "Exc\\n";
		private const string c_expectedFormattedExceptionMessage2 = "eption";

		[TestMethod]
		public void Format_OnlyMessage_ContainsMessageAndLogLevel()
		{
			var underTest = new DefaultLogFormatter();

			string result = underTest.Format(LogLevel.Verbose, c_testMessage);

			Assert.IsTrue(result.Contains(c_expectedFormattedMessage), "Formated log should contain message");
			Assert.IsTrue(result.Contains("Verbose"), "Formated log should log level");
		}

		[TestMethod]
		public void Format_MessageAndException_ContainsMessageLogLevelAndExceptionInfo()
		{
			Exception exceptionToLog;
			try
			{
				throw new ApplicationException(c_exceptionMessage);
			}
			catch(ApplicationException e)
			{
				exceptionToLog = e;
			}

			var underTest = new DefaultLogFormatter();

			string result = underTest.Format(LogLevel.Error, c_testMessage, exceptionToLog);

			Assert.IsTrue(result.Contains(c_expectedFormattedMessage), "Formated log should contain message");
			Assert.IsTrue(result.Contains(c_expectedFormattedExceptionMessage1), "Formated log should contain exception message part 1");
			Assert.IsTrue(result.Contains(c_expectedFormattedExceptionMessage2), "Formated log should contain exception message part 2");
			Assert.IsTrue(result.Contains(nameof(Format_MessageAndException_ContainsMessageLogLevelAndExceptionInfo)), "Formated log should contain stack trace with the method where exception was thrown");
			Assert.IsTrue(result.Contains(nameof(ApplicationException)), "Formated log should contain name of the thrown exception type");
			Assert.IsTrue(result.Contains("Error"), "Formated log should log level");
		}

		[TestMethod]
		public void Format_MessageAndData_ContainsMessageLogLevelAndData()
		{
			var logData = new LogData()
			{
				{ "SimpleKey", "SimpleValue" },
				{ "Extravaga\nt keeeyy!!!!?", "Val" },
				{ "keeeeyy", "Val\n!!!?:-!\adlpkf1209jm" },
				{ "key", null }
			};
			var underTest = new DefaultLogFormatter();

			string result = underTest.Format(LogLevel.Warning, c_testMessage, logData);

			Assert.IsTrue(result.Contains(c_expectedFormattedMessage), "Formated log should contain message");
			foreach(var keyValue in logData)
			{
				string expectedKey = keyValue.Key.Replace("\n","\\n");
				string expectedValue = keyValue.Value?.Replace("\n", "\\n") ?? "null";
				Assert.IsTrue(result.Contains(expectedKey), "Formated log should contain all keys from logData. Key=" + expectedKey);
				Assert.IsTrue(result.Contains(expectedValue), "Formated log should contain all values from logData. Value=" + expectedValue);
			}
			Assert.IsTrue(result.Contains("Warning"), "Formated log should log level");
		}
	}
}
