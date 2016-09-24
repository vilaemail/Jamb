using Jamb.Logging;
using JambTests.Assertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace JambTests.Logging
{
	[TestClass]
	public class UnitTestDefaultLogger
	{
		[TestMethod]
		public void Log_WithLowerLevelThanMinimum_NothingIsLogged()
		{
			// Because we are using strict mocks we are ensuring they are not called not a single time
			Mock<ILogConsumer> stubConsumer = new Mock<ILogConsumer>(MockBehavior.Strict);
			Mock<ILogFormatter> stubFormatter = new Mock<ILogFormatter>(MockBehavior.Strict);
			LogLevel minimumLogLevel = LogLevel.Info;
			var underTest = new DefaultLogger(minimumLogLevel, stubConsumer.Object, stubFormatter.Object);

			underTest.Log(LogLevel.Verbose, "blah");
			underTest.Log(LogLevel.Verbose, "blah", new ApplicationException());
			underTest.Log(LogLevel.Debug, "blah", () => { throw new Exception("We shouldn't generate log data if we shouldn't log"); });
		}

		[TestMethod]
		public void Log_WithNullParameter_ExceptionThrown()
		{
			// Because we are using strict mocks we are ensuring they are not called not a single time
			Mock<ILogConsumer> stubConsumer = new Mock<ILogConsumer>(MockBehavior.Strict);
			Mock<ILogFormatter> stubFormatter = new Mock<ILogFormatter>(MockBehavior.Strict);
			LogLevel minimumLogLevel = LogLevel.Info;
			var underTest = new DefaultLogger(minimumLogLevel, stubConsumer.Object, stubFormatter.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.Log(LogLevel.Error, null), typeof(ArgumentNullException), "We should throw when message is null");
			AssertHelper.AssertExceptionHappened(() => underTest.Log(LogLevel.Error, "message", e: null), typeof(ArgumentNullException), "We should throw when exception is null");
			AssertHelper.AssertExceptionHappened(() => underTest.Log(LogLevel.Error, "message", logDataCreator: null), typeof(ArgumentNullException), "We should throw when logDataCreator is null");
			AssertHelper.AssertExceptionHappened(() => underTest.Log(LogLevel.Error, null, new ApplicationException()), typeof(ArgumentNullException), "We should throw when message is null");
			AssertHelper.AssertExceptionHappened(() => underTest.Log(LogLevel.Error, null, () => new LogData()), typeof(ArgumentNullException), "We should throw when message is null");
		}

		[TestMethod]
		public void Log_NormalValues_ConsumerCalledWithFormattedString()
		{
			Mock<ILogConsumer> mockConsumer = new Mock<ILogConsumer>(MockBehavior.Strict);
			mockConsumer.Setup(obj => obj.AddLogEntry(It.IsRegex("logstr[1-3]")));
			Mock<ILogFormatter> stubFormatter = new Mock<ILogFormatter>(MockBehavior.Strict);
			int number = 1;
			stubFormatter.Setup(obj => obj.Format(It.IsAny<LogLevel>(), It.IsRegex("message[1-3]"))).Returns(() => "logstr" + number++);
			stubFormatter.Setup(obj => obj.Format(It.IsAny<LogLevel>(), It.IsRegex("message[1-3]"), It.IsAny<Exception>())).Returns(() => "logstr" + number++);
			stubFormatter.Setup(obj => obj.Format(It.IsAny<LogLevel>(), It.IsRegex("message[1-3]"), It.IsAny<LogData>())).Returns(() => "logstr" + number++);
			LogLevel minimumLogLevel = LogLevel.Info;
			var underTest = new DefaultLogger(minimumLogLevel, mockConsumer.Object, stubFormatter.Object);

			underTest.Log(LogLevel.Info, "message1");
			mockConsumer.Verify(obj => obj.AddLogEntry(It.IsRegex("logstr[1-3]")), Times.Once);
			mockConsumer.ResetCalls();

			underTest.Log(LogLevel.Warning, "message2", new ApplicationException());
			mockConsumer.Verify(obj => obj.AddLogEntry(It.IsRegex("logstr[1-3]")), Times.Once);
			mockConsumer.ResetCalls();

			underTest.Log(LogLevel.Error, "message3", () => new LogData());
			mockConsumer.Verify(obj => obj.AddLogEntry(It.IsRegex("logstr[1-3]")), Times.Once);
		}

		[TestMethod]
		public void Log_LogDataFuncThrowsException_FormatsOriginalMessageWithExceptionInfo()
		{
			Mock<ILogConsumer> stubConsumer = new Mock<ILogConsumer>(MockBehavior.Loose);
			Mock<ILogFormatter> mockFormatter = new Mock<ILogFormatter>(MockBehavior.Loose);
			LogLevel minimumLogLevel = LogLevel.Info;
			var underTest = new DefaultLogger(minimumLogLevel, stubConsumer.Object, mockFormatter.Object);

			underTest.Log(LogLevel.Info, "message", () => { throw new ApplicationException("unit test induced"); });
			
			Func<LogData, bool> verifyLogData = (LogData data) =>
			{
				if (data.Count != 1) return false;
				return data.Select(x => x.Value).First().Contains("unit test induced");
			};
			mockFormatter.Verify(obj => obj.Format(LogLevel.Info, "message", It.Is<LogData>((data) => verifyLogData(data))), Times.Once);
		}

		[TestMethod]
		public void Log_TryToLogWithinALog_InnerLogCallIgnored()
		{
			Mock<ILogConsumer> mockConsumer = new Mock<ILogConsumer>(MockBehavior.Loose);
			Mock<ILogFormatter> mockFormatter = new Mock<ILogFormatter>(MockBehavior.Strict);
			mockFormatter.Setup(obj => obj.Format(LogLevel.Info, It.IsRegex(".*outerLog.*"), It.IsAny<LogData>())).Returns("dummy"); // Make sure we log only outer log
			LogLevel minimumLogLevel = LogLevel.Info;
			var underTest = new DefaultLogger(minimumLogLevel, mockConsumer.Object, mockFormatter.Object);

			underTest.Log(LogLevel.Info, "outerLog", () => {
				underTest.Log(LogLevel.Error, "logWithinALog");
				return new LogData();
			});

			mockConsumer.Verify(obj => obj.AddLogEntry(It.IsAny<string>()), Times.Once); // Make sure we logged only once
		}

		[TestMethod]
		public void Dispose_WhenDisposing_ConsumerIsDisposed()
		{
			Mock<ILogConsumer> mockConsumer = new Mock<ILogConsumer>(MockBehavior.Loose);
			Mock<ILogFormatter> stubFormatter = new Mock<ILogFormatter>(MockBehavior.Strict);
			LogLevel minimumLogLevel = LogLevel.Info;
			var underTest = new DefaultLogger(minimumLogLevel, mockConsumer.Object, stubFormatter.Object);

			underTest.Dispose();

			mockConsumer.Verify(obj => obj.Dispose(), Times.Once);
		}

	}
}
