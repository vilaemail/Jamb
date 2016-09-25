using Jamb.Values;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JambTests.Values
{
	[TestClass]
	public class UnitTestSettings
	{
		[TestCleanup]
		public void Cleanup()
		{
			Settings.SetImplementation(null);
		}

		[TestMethod]
		public void Instance_AtBeginning_IsNullLogger()
		{
			Assert.AreEqual(typeof(NullValueProvider<SettingsKey>), Settings.Instance.GetType());
		}

		[TestMethod]
		public void Instance_WhenSetToNull_IsNullLogger()
		{
			Settings.SetImplementation(null);
			Assert.AreEqual(typeof(NullValueProvider<SettingsKey>), Settings.Instance.GetType());
		}

		[TestMethod]
		public void Instance_WhenSet_IsWhatWasSet()
		{
			var stubedValuesProvider = new Mock<IValuesProvider<SettingsKey>>().Object;
			Settings.SetImplementation(stubedValuesProvider);
			Assert.AreEqual(stubedValuesProvider, Settings.Instance, "When set 1st instance it should be 1st instance");

			var stubedValuesProvider2 = new Mock<IValuesProvider<SettingsKey>>().Object;
			Settings.SetImplementation(stubedValuesProvider2);
			Assert.AreEqual(stubedValuesProvider2, Settings.Instance, "When set 2nd instance it should be 2nd instance");
			Assert.AreNotEqual(stubedValuesProvider, Settings.Instance, "When set 2nd instance it should not be 1st instance");
		}
	}
}
