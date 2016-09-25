using Jamb.Values;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JambTests.Values
{
	[TestClass]
	public class UnitTestInMemoryValue
	{
		[TestMethod]
		public void Get_WhenCalled_ReturnsInitialValue()
		{
			var underTest = new InMemoryValue<string>("blah");

			string returnedValue = underTest.Get();

			Assert.AreEqual("blah", returnedValue);
		}

		[TestMethod]
		public void Set_WhenCalled_ChangesTheValue()
		{
			var underTest = new InMemoryValue<string>("blah");

			underTest.Set("new");
			string returnedValue = underTest.Get();

			Assert.AreEqual("new", returnedValue);
		}
	}
}
