using Jamb.Common;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;
using JambTests.Assertion;

namespace JambTests.Common
{
	[TestClass]
	public class UnitTestTaskHelper
	{
		[TestMethod]
		public void WaitAndThrowActualException_NoReturnValue_ReturnsWhenTaskIsFinished()
		{
			// Dummy variable containing a value
			int i = 0;

			// Start task that sleeps some time and then changes value to 1
			Task.Run(() =>
			{
				// Wait for some time
				System.Threading.Thread.Sleep(500);
				Assert.AreEqual(0, i, "Task should be the first to change this value");
				// Change the value
				i = 1;
			}).WaitAndThrowActualException();

			Assert.AreEqual(1, i, "We should have waited for the task to finish before proceeding");
			// Change value
			i = 2;
		}

		[TestMethod]
		public void WaitAndThrowActualException_WithReturnValue_ReturnsWhenTaskIsFinishedAndTheTasksResult()
		{
			// Dummy variable containing a value
			int i = 0;
			List<int> returnValue = new List<int>() { 5, 6 };

			// Start task that sleeps some time and then changes value to 1
			List<int> returned = Task.Run(() =>
			{
				// Wait for some time
				System.Threading.Thread.Sleep(500);
				Assert.AreEqual(0, i, "Task should be the first to change this value");
				// Change the value
				i = 1;
				// Return
				return returnValue;
			}).WaitAndThrowActualException();

			Assert.AreEqual(returnValue, returned, "We should have received return value of the task");
			Assert.AreEqual(1, i, "We should have waited for the task to finish before proceeding");
			// Change value
			i = 2;
		}

		[TestMethod]
		public void WaitAndThrowActualException_TaskThrowsException_RethrowsSameException()
		{
			AssertHelper.AssertExceptionHappened(() =>
			{
				Task.Run(() => { throw new ApplicationException("Test exception"); }).WaitAndThrowActualException();
			}, typeof(ApplicationException), "We should have rethrowed the original exception (task that doesn't return value)");

			AssertHelper.AssertExceptionHappened(() =>
			{
				Func<Task<List<int>>> throwingTask = delegate () { throw new ApplicationException("Test exception"); };

				Task.Run(throwingTask).WaitAndThrowActualException();
			}, typeof(ApplicationException), "We should have rethrowed the original exception (task that returns value)");
		}
	}
}
