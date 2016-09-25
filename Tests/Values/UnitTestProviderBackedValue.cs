using Jamb.Values;
using JambTests.Assertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace JambTests.Values
{
	[TestClass]
	public class UnitTestProviderBackedValue
	{
		private const string c_expectedKey = "Key";
		private const int c_expectedValue = 42;

		[TestMethod]
		public void Construct_WithNullArgument_ThrowsException()
		{
			AssertHelper.AssertExceptionHappened(() => new ProviderBackedValue<string, int>(null, c_expectedKey), typeof(ArgumentNullException), "We should throw if provider is null");
			AssertHelper.AssertExceptionHappened(() => new ProviderBackedValue<string, int>(Mock.Of<IValuesProvider<string>>(), null), typeof(ArgumentNullException), "We should throw if key is null");
		}

		[TestMethod]
		public void Get_WhenCalled_ReturnsValueFromProvider()
		{
			Mock<IValuesProvider<string>> mockProvider = new Mock<IValuesProvider<string>>(MockBehavior.Strict);
			int callCount = 0;
			mockProvider.Setup(obj => obj.Get<int>(c_expectedKey)).Returns(() => c_expectedValue + callCount++);
			var underTest = new ProviderBackedValue<string, int>(mockProvider.Object, c_expectedKey);

			// Call once
			int returnedValue = underTest.Get();

			Assert.AreEqual(c_expectedValue, returnedValue);
			mockProvider.Verify(obj => obj.Get<int>(c_expectedKey), Times.Once);
			mockProvider.ResetCalls();

			// Call second time and make sure we return the new value
			int returnedValue2 = underTest.Get();

			Assert.AreEqual(c_expectedValue + 1, returnedValue2);
			mockProvider.Verify(obj => obj.Get<int>(c_expectedKey), Times.Once);
		}
	}
}
