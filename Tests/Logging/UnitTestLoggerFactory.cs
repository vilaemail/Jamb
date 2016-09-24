using Jamb.Logging;
using JambTests.Assertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace JambTests.Logging
{
	[TestClass]
	public class UnitTestLoggerFactory
	{
		[TestMethod]
		public void Create_WithMockedInterfaces_CreatesDefaultLogger()
		{
			var result = new LoggerFactory().Create(LogLevel.Info, Mock.Of<ILogConsumer>(), Mock.Of<ILogFormatter>());

			Assert.IsNotNull(result);
			Assert.AreEqual(typeof(DefaultLogger), result.GetType());
		}

		[TestMethod]
		public void Create_WithNullValue_ThrowsException()
		{
			AssertHelper.AssertExceptionHappened(() => new LoggerFactory().Create(LogLevel.Info, null, Mock.Of<ILogFormatter>()), typeof(ArgumentNullException), "For null LogConsumer we should throw");
			AssertHelper.AssertExceptionHappened(() => new LoggerFactory().Create(LogLevel.Info, Mock.Of<ILogConsumer>(), null), typeof(ArgumentNullException), "For null LogFormatter we should throw");
			AssertHelper.AssertExceptionHappened(() => new LoggerFactory().Create(LogLevel.Info, null, null), typeof(ArgumentNullException), "For null LogConsumer and LogFormatter we should throw");
		}
	}
}
