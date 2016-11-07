using Jamb.Common;
using Jamb.Communication;
using Jamb.Communication.Network;
using Jamb.Communication.WireProtocol;
using Jamb.Values;
using JambTests.Assertion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JambTests.Communication
{
	[TestClass]
	public class UnitTestConnection
	{
		private Action<Connection, Mock<IMessagePasser>, int> c_assertMessagePasserDisposed = new Action<Connection, Mock<IMessagePasser>, int>((connection, mockMessagePasser, testCaseNumber) =>
		{
			mockMessagePasser.Verify(obj => obj.Dispose(), Times.Once, "Message passer should be disposed of. Test case number: " + testCaseNumber);
		});

		[TestMethod]
		public void Constructor_WhenCreated_InitialStateIsLost()
		{
			Connection underTest = CreateForTest();

			Assert.AreEqual(ConnectionState.Lost, underTest.State);
		}

		[TestMethod]
		public void SupportsSendingSupportsReceiving_WhenCreated_WeSupportSendingAndReceiving()
		{
			Connection underTest = CreateForTest();

			Assert.AreEqual(true, underTest.SupportsSending(), "We should support sending initialy");
			Assert.AreEqual(true, underTest.SupportsReceiving(), "We should support sending initialy");
		}

		[TestMethod]
		public void SupportsSending_ForAllPossibleStates_ExpectedReturnValue()
		{
			var testCases = new List<Tuple<ConnectionState, Action<Connection>>>()
			{
				Tuple.Create<ConnectionState, Action<Connection>>(ConnectionState.Lost, (connection) => Assert.AreEqual(true, connection.SupportsSending())),
				Tuple.Create<ConnectionState, Action<Connection>>(ConnectionState.Open, (connection) => Assert.AreEqual(true, connection.SupportsSending())),
				Tuple.Create<ConnectionState, Action<Connection>>(ConnectionState.Closing, (connection) => Assert.AreEqual(true, connection.SupportsSending())),
				Tuple.Create<ConnectionState, Action<Connection>>(ConnectionState.Closed, (connection) => Assert.AreEqual(false, connection.SupportsSending()))
			};

			TestState(testCases);
		}

		[TestMethod]
		public void SupportsReceiving_ForAllPossibleStates_ExpectedReturnValue()
		{
			var testCases = new List<Tuple<ConnectionState, Action<Connection>>>()
			{
				Tuple.Create<ConnectionState, Action<Connection>>(ConnectionState.Lost, (connection) => Assert.AreEqual(true, connection.SupportsReceiving())),
				Tuple.Create<ConnectionState, Action<Connection>>(ConnectionState.Open, (connection) => Assert.AreEqual(true, connection.SupportsReceiving())),
				Tuple.Create<ConnectionState, Action<Connection>>(ConnectionState.Closing, (connection) => Assert.AreEqual(false, connection.SupportsReceiving())),
				Tuple.Create<ConnectionState, Action<Connection>>(ConnectionState.Closed, (connection) => Assert.AreEqual(false, connection.SupportsReceiving()))
			};

			TestState(testCases);
		}

		[TestMethod]
		public void MarkAsLost_ForAllPossibleStartingStates_ChangesToExpectedState()
		{
			var testCases = new List<Tuple<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>>()
			{
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Lost, ConnectionState.Lost, null),
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Open, ConnectionState.Lost, null),
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Closing, ConnectionState.Closed, c_assertMessagePasserDisposed),
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Closed, ConnectionState.Closed, null)
			};

			TestStateChanges(testCases, (underTest) => underTest.MarkAsLost());
		}

		[TestMethod]
		public void BeginClosing_ForAllPossibleStartingStates_ChangesToExpectedState()
		{
			var testCases = new List<Tuple<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>>()
			{
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Lost, ConnectionState.Closed, null),
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Open, ConnectionState.Closing, null),
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Closing, ConnectionState.Closing, null),
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Closed, ConnectionState.Closed, null)
			};

			TestStateChanges(testCases, (underTest) => underTest.BeginClosing());
		}

		[TestMethod]
		public void Terminate_WhenCalled_ChangesStateToClosedAndDisposesOfMessagePasserIfPresent()
		{
			var testCases = new List<Tuple<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>>()
			{
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Lost, ConnectionState.Closed, null),
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Open, ConnectionState.Closed, c_assertMessagePasserDisposed),
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Closing, ConnectionState.Closed, c_assertMessagePasserDisposed),
				Tuple.Create<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>(ConnectionState.Closed, ConnectionState.Closed, null)
			};

			TestStateChanges(testCases, (underTest) => underTest.Terminate());
		}

		/// <summary>
		/// Creates an instance of SUT with the given ConnectionState for each of the test cases.
		/// Calls the assert func for each testcase with the constructed SUT.
		/// </summary>
		/// <param name="testCases">Contain initial state and assert function to perform</param>
		private static void TestState(List<Tuple<ConnectionState, Action<Connection>>> testCases)
		{
			foreach(var testCase in testCases)
			{
				ConnectionState state = testCase.Item1;
				Action<Connection> assertFunc = testCase.Item2;

				Connection underTest = CreateForTest(state);

				assertFunc(underTest);
			}
		}

		/// <summary>
		/// Tests state changes. For each test case sets up the system under test to specified start state.
		/// Performs given testAction.
		/// Asserts resulting state to be equal to provided expected end state.
		/// </summary>
		/// <param name="testCases">List of test case tuples which members are start state, expected end state and optional action for additional asserts. The action accepts SUT, mocked MessagePasser and test case number.</param>
		/// <param name="testAction">Action to invoke for each test case</param>
		/// <param name="additionalAsserts"></param>
		private static void TestStateChanges(List<Tuple<ConnectionState, ConnectionState, Action<Connection, Mock<IMessagePasser>, int>>> testCases, Action<Connection> testAction)
		{
			int i = 0;
			foreach (var testCase in testCases)
			{
				ConnectionState startState = testCase.Item1;
				ConnectionState expectedEndState = testCase.Item2;
				Action<Connection, Mock<IMessagePasser>, int> additionalAssert = testCase.Item3 ?? new Action<Connection, Mock<IMessagePasser>, int>((con, mp, tcn) => { });

				Mock<IMessagePasser> messagePasserMock = new Mock<IMessagePasser>(MockBehavior.Loose);
				messagePasserMock.Setup(obj => obj.Dispose());
				Connection underTest = CreateForTest(startState, messagePasserMock);

				testAction(underTest);

				Assert.AreEqual(expectedEndState, underTest.State, "Unexpected state. Test case " + i);
				additionalAssert(underTest, messagePasserMock, i);
				i++;
			}
		}

		/// <summary>
		/// Creates an instance of Connection in the requested state.
		/// </summary>
		private static Connection CreateForTest(ConnectionState initialState = ConnectionState.Lost, Mock<IMessagePasser> mockMessagePasser = null)
		{
			Connection forTest = new Connection(new SyncTaskFactory());

			if(mockMessagePasser == null)
			{
				mockMessagePasser = new Mock<IMessagePasser>(MockBehavior.Loose);
			}

			switch(initialState)
			{
				case ConnectionState.Open:
					forTest.Reopen(mockMessagePasser.Object);
					break;
				case ConnectionState.Lost:
					break;
				case ConnectionState.Closing:
					forTest.Reopen(mockMessagePasser.Object);
					forTest.BeginClosing();
					break;
				case ConnectionState.Closed:
					forTest.Terminate();
					break;
				default:
					Assert.Fail("Unsupported connection state in test");
					break;
			}

			Assert.AreEqual(initialState, forTest.State, "Making sure helper method for construction works as expected");

			return forTest;
		}
	}
}
