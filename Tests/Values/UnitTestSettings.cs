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

		[TestMethod]
		public void Changable_WhenCalled_ReturnsSettingThatChangesWithValue()
		{
			string valuesProviderReturnValue = "1st time";
			var stubValuesProvider = new Mock<IValuesProvider<SettingsKey>>();
			stubValuesProvider.Setup(obj => obj.Get<string>(It.IsAny<SettingsKey>())).Returns(() => valuesProviderReturnValue);
			Settings.SetImplementation(stubValuesProvider.Object);

			IValue<string> setting = Settings.Changable<string>(SettingsKey.CommunicationBackoffInMs);

			Assert.AreEqual(valuesProviderReturnValue, setting.Get());
			valuesProviderReturnValue = "2nd time";
			Assert.AreEqual(valuesProviderReturnValue, setting.Get());
		}

		[TestMethod]
		public void FromCurrentValue_WhenCalled_ReturnsSettingThatKeepsValueFromTheCreationTime()
		{
			string valuesProviderReturnValue = "1st time";
			string expectedReturnValue = "1st time";
			var stubValuesProvider = new Mock<IValuesProvider<SettingsKey>>();
			stubValuesProvider.Setup(obj => obj.Get<string>(It.IsAny<SettingsKey>())).Returns(() => valuesProviderReturnValue);
			Settings.SetImplementation(stubValuesProvider.Object);

			IValue<string> setting = Settings.FromCurrentValue<string>(SettingsKey.CommunicationBackoffInMs);

			Assert.AreEqual(expectedReturnValue, setting.Get());
			valuesProviderReturnValue = "2nd time";
			Assert.AreEqual(expectedReturnValue, setting.Get());
		}

		[TestMethod]
		public void CurrentValue_WhenCalled_ReturnsCurrentValue()
		{
			string valuesProviderReturnValue = "";
			var stubValuesProvider = new Mock<IValuesProvider<SettingsKey>>();
			stubValuesProvider.Setup(obj => obj.Get<string>(It.IsAny<SettingsKey>())).Returns(() => valuesProviderReturnValue);
			Settings.SetImplementation(stubValuesProvider.Object);

			valuesProviderReturnValue = "1st time";
			string settingValue = Settings.CurrentValue<string>(SettingsKey.CommunicationBackoffInMs);
			Assert.AreEqual(valuesProviderReturnValue, settingValue);

			valuesProviderReturnValue = "2nd time";
			settingValue = Settings.CurrentValue<string>(SettingsKey.CommunicationBackoffInMs);
			Assert.AreEqual(valuesProviderReturnValue, settingValue);
		}

	}
}
