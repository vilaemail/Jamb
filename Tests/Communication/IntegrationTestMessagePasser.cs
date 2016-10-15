using Jamb.Common;
using Jamb.Communication;
using Jamb.Communication.WireProtocol;
using Jamb.Values;
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
	/// Tests that MessagePasser and underlying classes work as expected with.
	/// 
	/// NOTE: These tests use actual system network resources!
	/// </summary>
	[TestClass]
	public class ComponentTestMessagePasser
	{
		[TestCategory("Integration"), TestCategory("Longrunning"), TestMethod]
		public void BasicServerClient_ServerAndClientExchangeOneMessageAndCloseConnection_MessagesIntactGracefullExit()
		{
			// Make sure all our blocking actions finish eventually
			CancellationTokenSource cts = new CancellationTokenSource();
			cts.CancelAfter(10000);

			// Get server and client message passers
			var serverAndClient = GetServerAndClientMessagePasser();
			var server = serverAndClient.Item1;
			var client = serverAndClient.Item2;

			// Execute test
			try
			{
				server.SendMessage(UnitTestMessagePasser.CreateNestedMessage(122), cts.Token);
				UnitTestMessage received = client.ReceiveMessage(cts.Token) as UnitTestMessage;
				UnitTestMessagePasser.AssertNestedMessage(received, 122);
			}
			finally
			{
				// Make sure we dispose our resources
				server?.Dispose();
				client?.Dispose();
			}
		}

		[TestCategory("Integration"), TestCategory("Longrunning"), TestMethod]
		public void BasicServerClient_ClientWaitsForMessageAndServerSendsIt_MessagesIntactGracefullExit()
		{
			// Make sure all our blocking actions finish eventually
			CancellationTokenSource cts = new CancellationTokenSource();
			cts.CancelAfter(10000);

			// Get server and client message passers
			var serverAndClient = GetServerAndClientMessagePasser();
			var server = serverAndClient.Item1;
			var client = serverAndClient.Item2;

			// Execute test
			try
			{
				Task<UnitTestMessage> received = Task.Run(() => client.ReceiveMessage(cts.Token) as UnitTestMessage);
				Thread.Sleep(1000);
				Assert.IsFalse(received.IsCompleted, "We can't complete before message is sent");
				server.SendMessage(UnitTestMessagePasser.CreateNestedMessage(122), cts.Token);
				
				UnitTestMessagePasser.AssertNestedMessage(received.Result, 122);
			}
			finally
			{
				// Make sure we dispose our resources
				server?.Dispose();
				client?.Dispose();
			}
		}

		[TestCategory("Integration"), TestCategory("Longrunning"), TestMethod]
		public void StressServerClient_ClientAndServerBothSendALotOfMessagesInParallel_MessagesIntactGracefullExit()
		{
			// Make sure all our blocking actions finish eventually
			CancellationTokenSource cts = new CancellationTokenSource();
			cts.CancelAfter(10000);

			// Get server and client message passers
			var serverAndClient = GetServerAndClientMessagePasser();
			var server = serverAndClient.Item1;
			var client = serverAndClient.Item2;

			// Execute test
			try
			{
				Task serverReceiver = Task.Run(() => ReceiveMessages(server, cts.Token, 300, 45, 50));
				Task clientReceiver = Task.Run(() => ReceiveMessages(client, cts.Token, 300, 50, 30));
				Task serverSender = Task.Run(() => SendMessages(client, cts.Token, 300, 50, 10));
				Task clientSender = Task.Run(() => SendMessages(server, cts.Token, 300, 27, 50));

				Task.WhenAll(clientReceiver, clientSender, serverReceiver, serverSender).WaitAndThrowActualException();
			}
			finally
			{
				// Make sure we dispose our resources
				server?.Dispose();
				client?.Dispose();
			}
		}

		private static void SendMessages(MessagePasser channel, CancellationToken cancelToken, int messageCount = 300, int sleepEvery = 0, int sleepTime = 0)
		{
			for (int i = 0; i < messageCount; i++)
			{
				channel.SendMessage(UnitTestMessagePasser.CreateNestedMessage(i), cancelToken);
				if (sleepEvery > 0 && i % sleepEvery == 0)
					Thread.Sleep(sleepTime);
			}
		}

		private static void ReceiveMessages(MessagePasser channel, CancellationToken cancelToken, int messageCount = 300, int sleepEvery = 0, int sleepTime = 0)
		{
			for (int i = 0; i < messageCount; i++)
			{
				var message = channel.ReceiveMessage(cancelToken) as UnitTestMessage;
				UnitTestMessagePasser.AssertNestedMessage(message, i);

				if (sleepEvery > 0 && i % sleepEvery == 0)
					Thread.Sleep(sleepTime);
			}
		}

		internal static Tuple<MessagePasser, MessagePasser> GetServerAndClientMessagePasser()
		{
			var serverAndClientStream = IntegrationTestNetworkStream.GetServerAndClientStream();

			var serverStream = serverAndClientStream.Item1;
			var clientStream = serverAndClientStream.Item2;

			return new Tuple<MessagePasser, MessagePasser>(new MessagePasser(serverStream, InMemoryValue<int>.Is(10240), InMemoryValue<int>.Is(500)), new MessagePasser(clientStream, InMemoryValue<int>.Is(10240), InMemoryValue<int>.Is(500)));
		}

	}
}
