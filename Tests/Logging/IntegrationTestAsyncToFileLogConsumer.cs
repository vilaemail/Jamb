using Jamb.Common;
using Jamb.Communication;
using Jamb.Communication.WireProtocol;
using JambTests.Assertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JambTests.Communication
{
	/// <summary>
	/// 
	/// 
	/// NOTE: These tests use actual file system!
	/// </summary>
	[TestClass]
	public class ComponentTestAsyncToFileLogConsumer
	{
		[TestCategory("Integration"), TestCategory("Longrunning"), TestMethod]
		public void Todo()
		{
			// TODO: Write the tests
		}
	}
}
