using Jamb.Logging;
using Jamb.Values;
using JambTests.Assertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Configuration;
using System.Linq;

namespace JambTests.Values
{
	[TestClass]
	public class UnitTestDictionaryProvider
	{
		private const string c_expectedKey = "Key";
		private const int c_expectedValue = 42;

		[TestMethod]
		public void Set_NullKey_ThrowsException()
		{
			var underTest = new DictionaryProvider<string>();

			AssertHelper.AssertExceptionHappened(() => underTest.Set<int>(null, c_expectedValue), typeof(ArgumentNullException), "We should throw on null key");
		}

		[TestMethod]
		public void Set_WhenCalled_SetsTheSetting()
		{
			var underTest = new DictionaryProvider<string>();

			underTest.Set<int>(c_expectedKey, c_expectedValue);

			int value = underTest.Get<int>(c_expectedKey);
			Assert.AreEqual(c_expectedValue, value);
		}

		[TestMethod]
		public void Get_NullKey_ThrowsException()
		{
			var underTest = new DictionaryProvider<string>();

			AssertHelper.AssertExceptionHappened(() => underTest.Get<int>(null), typeof(ArgumentNullException), "We should throw on null key");
		}

		[TestMethod]
		public void Get_WhenCalledWithExistingSetting_ReturnsTheSetting()
		{
			var underTest = new DictionaryProvider<string>();
			underTest.Set(c_expectedKey, c_expectedValue);

			int returnedValue = underTest.Get<int>(c_expectedKey);

			Assert.AreEqual(c_expectedValue, returnedValue);
		}

		[TestMethod]
		public void Get_WhenCalledWithExistingSettingButWrongType_ThrowsException()
		{
			var underTest = new DictionaryProvider<string>();
			underTest.Set(c_expectedKey, c_expectedValue);

			AssertHelper.AssertExceptionHappened(() => underTest.Get<string>(c_expectedKey), typeof(ValuesException), "We should throw for wrong expected type");
		}

		[TestMethod]
		public void Get_WhenCalledWithNonExistingSetting_ThrowsException()
		{
			var underTest = new DictionaryProvider<string>();

			AssertHelper.AssertExceptionHappened(() => underTest.Get<string>(c_expectedKey), typeof(ValuesException), "We should throw for non-existent setting");
		}
	}
}
