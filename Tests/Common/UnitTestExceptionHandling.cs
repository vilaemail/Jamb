using Jamb.Common;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JambTests.Assertion;
using System.Text.RegularExpressions;

namespace JambTests.Common
{
	[TestClass]
	public class UnitTestExceptionHandling
	{
		[TestMethod]
		public void CreateStringDescribingException_NullArgument_Throws()
		{
			AssertHelper.AssertExceptionHappened(() => ExceptionHandling.CreateStringDescribingException(null), typeof(ArgumentNullException), "We should have thrown when we receive null argument");
		}

		[TestMethod]
		public void CreateStringDescribingException_ExceptionWithoutMessage_ReturnsExpectedString()
		{
			const string expectedResult = "Exception Found:\nType: System.ApplicationException\nMessage: Exception of type 'System.ApplicationException' was thrown.\nSource: \nStacktrace: ";
			string result = ExceptionHandling.CreateStringDescribingException(new ApplicationException(null));
			Assert.AreEqual(expectedResult, result, "We should have described exception as expected.");
		}

		[TestMethod]
		public void CreateStringDescribingException_SimpleException_ReturnsExpectedString()
		{
			const string expectedResult = "Exception Found:\nType: System.ApplicationException\nMessage: Test exception message\nSource: \nStacktrace: ";
			string result = ExceptionHandling.CreateStringDescribingException(new ApplicationException("Test exception message"));
			Assert.AreEqual(expectedResult, result, "We should have described exception as expected.");
		}

		[TestMethod]
		public void CreateStringDescribingException_NestedException_ReturnsExpectedString()
		{
			const string expectedResult = "Exception Found:\nType: System.ApplicationException\nMessage: Test exception message\nSource: \nStacktrace: \n  Inner Exception Found:\n  Type: System.ArgumentNullException\n  Message: Test inner exception message\r\n  Message: Parameter name: paramname\n  Source: \n  Stacktrace: ";
			string result = ExceptionHandling.CreateStringDescribingException(new ApplicationException("Test exception message", new ArgumentNullException("paramname", "Test inner exception message")));
			Assert.AreEqual(expectedResult, result, "We should have described exception as expected.");
		}

		[TestMethod]
		public void CreateStringDescribingException_SimpleActualException_ReturnsExpectedString()
		{
			const string expectedResultRegex =
				@"Exception Found:\nType: System\.ApplicationException\nMessage: Test exception message\nSource: JambTests\nStacktrace:    at JambTests\.Common\.UnitTestExceptionHandling\.CreateStringDescribingException_SimpleActualException_ReturnsExpectedString\(\) in [a-zA-Z]:[^:]+:line [0-9]+";
			Exception catchedException = null;
			try
			{
				throw new ApplicationException("Test exception message");
			}
			catch (Exception e)
			{
				catchedException = e;
			}
			string result = ExceptionHandling.CreateStringDescribingException(catchedException);
			Regex matcher = new Regex(expectedResultRegex);
			Assert.IsTrue(matcher.IsMatch(result), "We should have described exception as expected.");
		}

		[TestMethod]
		public void CreateStringDescribingException_NestedActualException_ReturnsExpectedString()
		{
			const string expectedResultRegex =
				@"Exception Found:\nType: System\.ApplicationException\nMessage: Test exception message\nSource: JambTests\nStacktrace:    at JambTests\.Common\.UnitTestExceptionHandling\.CreateStringDescribingException_NestedActualException_ReturnsExpectedString\(\) in [a-zA-Z]:[^:]+:line [0-9]+\n  Inner Exception Found:\n  Type: System\.ArgumentNullException\n  Message: Test inner exception message\r?\n  Message: Parameter name: paramname\n  Source: JambTests\n  Stacktrace:    at [a-zA-Z\._]*\([a-zA-Z\. ]*\) in [a-zA-Z]:[^:]+:line [0-9]+(\r\n  Stacktrace:    at JambTests.Common.UnitTestExceptionHandling.CreateStringDescribingException_NestedActualException_ReturnsExpectedString\(\) in [a-zA-Z]:[^:]+:line [0-9]+)?";
			Exception catchedException = null;
			try
			{
				try
				{
					ExceptionThrower(new ArgumentNullException("paramname", "Test inner exception message"));
				}
				catch (Exception inner)
				{
					throw new ApplicationException("Test exception message", inner);
				}

			}
			catch (Exception e)
			{
				catchedException = e;
			}
			string result = ExceptionHandling.CreateStringDescribingException(catchedException);
			Regex matcher = new Regex(expectedResultRegex);
			Assert.IsTrue(matcher.IsMatch(result), "We should have described exception as expected.");
		}

		/// <summary>
		/// Helper method to throw an exception. Used so that we get depth in call stack of exception.
		/// </summary>
		private static void ExceptionThrower(Exception e)
		{
			throw e;
		}
	}
}
