using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Assertion
{
    /// <summary>
    /// Contains helper methods for asserting.
    /// </summary>
    static class AssertHelper
    {
        /// <summary>
        /// Call an operation and assert operation throws exception of a given type. Provided custom failure message is added to the assert message.
        /// </summary>
        public static void AssertExceptionHappened(Action operation, Type expectedExceptionType, string failureMessage)
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
            catch(Exception e)
            {
                thrownException = e;
            }

            // Assert we had expected exception
            Assert.IsNotNull(thrownException, "No exception was thrown. Custom message: " + failureMessage);
            Assert.AreEqual(expectedExceptionType, thrownException.GetType(), "Exception if not of expected type. Custom message: " + failureMessage);
        }
    }
}
