using Jamb.Values;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JambTests.Values
{
	[TestClass]
	public class UnitTestInMemoryValue
	{
		[TestMethod]
		public void Get_WhenCalled_ReturnsInitialValue()
		{
			var underTests = new List<InMemoryValue<string>>() { new InMemoryValue<string>("blah"), InMemoryValue<string>.Is("blah") };

			int i = 0;
			foreach (var underTest in underTests)
			{
				string returnedValue = underTest.Get();

				Assert.AreEqual("blah", returnedValue, "Failed in test case #" + i);
				i++;
			}
		}

		[TestMethod]
		public void Set_WhenCalled_ChangesTheValue()
		{
			var underTests = new List<InMemoryValue<string>>() { new InMemoryValue<string>("blah"), InMemoryValue<string>.Is("blah") };

			int i = 0;
			foreach (var underTest in underTests)
			{
				underTest.Set("new");
				string returnedValue = underTest.Get();

				Assert.AreEqual("new", returnedValue, "Failed in test case #" + i);
				i++;
			}
		}
	}
}
