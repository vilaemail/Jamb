using Jamb.Common;
using JambTests.Assertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace JambTests.Common
{
	[TestClass]
	public class UnitTestArgumentValidation
	{
		[TestMethod]
		public void ValidateParametersForNullValues_NoArguments_ReturnsNormally()
		{
			ArgumentValidation.ValidateParametersForNullValues();
		}

		[TestMethod]
		public void ValidateParametersForNullValues_OneValidArgument_ReturnsNormally()
		{
			ArgumentValidation.ValidateParametersForNullValues(5);
			ArgumentValidation.ValidateParametersForNullValues("string");
			ArgumentValidation.ValidateParametersForNullValues(new List<int>());
		}

		[TestMethod]
		public void ValidateParametersForNullValues_MultipleValidArguments_ReturnsNormally()
		{
			ArgumentValidation.ValidateParametersForNullValues(5, "string", new List<int>() { 5, 6 });
		}

		[TestMethod]
		public void ValidateParametersForNullValues_OneInvalidArgument_Throws()
		{
			AssertHelper.AssertExceptionHappened(() => ArgumentValidation.ValidateParametersForNullValues(null), typeof(ArgumentNullException), "We should have thrown on null argument");
		}

		[TestMethod]
		public void ValidateParametersForNullValues_MultipleValidArgumentsAndOneInvalidArgument_Throws()
		{
			AssertHelper.AssertExceptionHappened(() => ArgumentValidation.ValidateParametersForNullValues(5, "string", new List<int>() { 5, 6 }, null), typeof(ArgumentNullException), "We should have thrown when we have at least one null argument");
		}
	}
}
