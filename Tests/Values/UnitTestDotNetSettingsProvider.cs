using Jamb.Values;
using JambTests.Assertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Configuration;

namespace JambTests.Values
{
	[TestClass]
	public class UnitTestDotNetSettingsProvider
	{
		private const string c_expectedKey = "Key";
		private const int c_expectedValue = 42;

		[TestMethod]
		public void Construct_WithNullParameter_ThrowsException()
		{
			AssertHelper.AssertExceptionHappened(() => new DotNetSettingsProvider<string>(null), typeof(ArgumentNullException), "We should throw on null argument");
		}

		[TestMethod]
		public void Get_NullKey_ThrowsException()
		{
			var underTest = new DotNetSettingsProvider<string>(Mock.Of<ApplicationSettingsBase>());

			AssertHelper.AssertExceptionHappened(() => underTest.Get<int>(null), typeof(ArgumentNullException), "We should throw on null key");
		}

		[TestMethod]
		public void Get_WhenCalledWithExistingSetting_ReturnsTheSetting()
		{
			Mock<ApplicationSettingsBase> mockDotNet = new Mock<ApplicationSettingsBase>(MockBehavior.Loose);
			mockDotNet.SetupGet(obj => obj[It.Is<string>((givenKey) => givenKey == c_expectedKey)]).Returns(c_expectedValue);
			var underTest = new DotNetSettingsProvider<string>(mockDotNet.Object);

			int returnedValue = underTest.Get<int>(c_expectedKey);

			Assert.AreEqual(c_expectedValue, returnedValue);
		}

		[TestMethod]
		public void Get_WhenCalledWithExistingSettingButWrongType_ThrowsException()
		{
			Mock<ApplicationSettingsBase> mockDotNet = new Mock<ApplicationSettingsBase>(MockBehavior.Loose);
			mockDotNet.SetupGet(obj => obj[It.Is<string>((givenKey) => givenKey == c_expectedKey)]).Returns(c_expectedValue);
			var underTest = new DotNetSettingsProvider<string>(mockDotNet.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.Get<string>(c_expectedKey), typeof(ValuesException), "We should throw for wrong expected type");
		}

		[TestMethod]
		public void Get_WhenCalledWithNonExistingSetting_ThrowsException()
		{
			Mock<ApplicationSettingsBase> mockDotNet = new Mock<ApplicationSettingsBase>(MockBehavior.Loose);
			mockDotNet.SetupGet(obj => obj[It.Is<string>((givenKey) => givenKey == c_expectedKey)]).Throws(new SettingsPropertyNotFoundException("Unit test induced"));
			var underTest = new DotNetSettingsProvider<string>(mockDotNet.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.Get<string>(c_expectedKey), typeof(ValuesException), "We should throw for non-existent setting");
		}

		[TestMethod]
		public void Get_WhenGettingASettingFails_ThrowsException()
		{
			Mock<ApplicationSettingsBase> mockDotNet = new Mock<ApplicationSettingsBase>(MockBehavior.Loose);
			mockDotNet.SetupGet(obj => obj[It.Is<string>((givenKey) => givenKey == c_expectedKey)]).Throws(new ApplicationException("Unit test induced"));
			var underTest = new DotNetSettingsProvider<string>(mockDotNet.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.Get<string>(c_expectedKey), typeof(ValuesException), "We should throw for failed setting retrieval operation");
		}

		[TestMethod]
		public void Set_NullKey_ThrowsException()
		{
			var underTest = new DotNetSettingsProvider<string>(Mock.Of<ApplicationSettingsBase>());

			AssertHelper.AssertExceptionHappened(() => underTest.Set<int>(null, c_expectedValue), typeof(ArgumentNullException), "We should throw on null key");
		}

		[TestMethod]
		public void Set_WhenCalled_SetsTheSetting()
		{
			Mock<ApplicationSettingsBase> mockDotNet = new Mock<ApplicationSettingsBase>(MockBehavior.Loose);
			mockDotNet.SetupSet(obj => obj[c_expectedKey] = c_expectedValue).Verifiable();
			var underTest = new DotNetSettingsProvider<string>(mockDotNet.Object);

			underTest.Set<int>(c_expectedKey, c_expectedValue);

			mockDotNet.VerifySet(obj => obj[c_expectedKey] = c_expectedValue);
		}

		[TestMethod]
		public void Set_WhenSettingDoesntExist_ThrowsException()
		{
			Mock<ApplicationSettingsBase> mockDotNet = new Mock<ApplicationSettingsBase>(MockBehavior.Loose);
			mockDotNet.SetupSet(obj => obj[c_expectedKey] = c_expectedValue).Throws(new SettingsPropertyNotFoundException("Unit test induced"));
			var underTest = new DotNetSettingsProvider<string>(mockDotNet.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.Set<int>(c_expectedKey, c_expectedValue), typeof(ValuesException), "We should throw for failed set of setting");
		}

		[TestMethod]
		public void Set_WhenSettingASettingFails_ThrowsException()
		{
			Mock<ApplicationSettingsBase> mockDotNet = new Mock<ApplicationSettingsBase>(MockBehavior.Loose);
			mockDotNet.SetupSet(obj => obj[c_expectedKey] = c_expectedValue).Throws(new ApplicationException("Unit test induced"));
			var underTest = new DotNetSettingsProvider<string>(mockDotNet.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.Set<int>(c_expectedKey, c_expectedValue), typeof(ValuesException), "We should throw for failed set of setting");
		}
	}
}
