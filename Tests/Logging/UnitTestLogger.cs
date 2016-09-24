using Jamb.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JambTests.Logging
{
	[TestClass]
	public class UnitTestLogger
	{
		[TestCleanup]
		public void Cleanup()
		{
			Logger.SetImplementation(null);
		}

		[TestMethod]
		public void Instance_AtBeginning_IsNull()
		{
			Assert.AreEqual(null, Logger.Instance);
		}

		[TestMethod]
		public void Instance_WhenSet_IsWhatWasSet()
		{
			var stubedLogger = new Mock<ILogger>().Object;
			Logger.SetImplementation(stubedLogger);
			Assert.AreEqual(stubedLogger, Logger.Instance, "When set 1st instance it should be 1st instance");

			var stubedLogger2 = new Mock<ILogger>().Object;
			Logger.SetImplementation(stubedLogger2);
			Assert.AreEqual(stubedLogger2, Logger.Instance, "When set 2nd instance it should be 2nd instance");
			Assert.AreNotEqual(stubedLogger, Logger.Instance, "When set 2nd instance it should not be 1st instance");
		}
	}
}
