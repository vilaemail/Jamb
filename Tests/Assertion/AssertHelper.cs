using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JambTests.Assertion
{
	/// <summary>
	/// Contains helper methods for asserting.
	/// </summary>
	static class AssertHelper
	{
		/// <summary>
		/// Call an operation and assert operation throws exception of a given type. Provided custom failure message is added to the assert message.
		/// If textExceptionMessageShouldContain is set, additionally asserts that exception message has the text provided.
		/// 
		/// Returns the exception that was thrown.
		/// </summary>
		public static Exception AssertExceptionHappened(Action operation, Type expectedExceptionType, string failureMessage, string textExceptionMessageShouldContain = "")
		{
			// Assert to sanity check our unit test
			Assert.IsNotNull(operation, "Given operation must not be null.");
			Assert.IsNotNull(expectedExceptionType, "Given expectedExceptionType must not be null.");
			Assert.IsNotNull(failureMessage, "Given failureMessage must not be null.");

			// Perform operation and catch exception (if any)
			Exception thrownException = null;

			try
			{
				operation();
			}
			catch (Exception e)
			{
				thrownException = e;
			}

			// Assert we had expected exception
			Assert.IsNotNull(thrownException, "No exception was thrown. Custom message: " + failureMessage);
			Assert.AreEqual(expectedExceptionType, thrownException.GetType(), "Exception if not of expected type. Custom message: " + failureMessage);
			Assert.IsTrue(thrownException.Message.Contains(textExceptionMessageShouldContain), "Exception should contain the text: " + textExceptionMessageShouldContain + ". Custom message: " + failureMessage);

			return thrownException;
		}
	}
}
