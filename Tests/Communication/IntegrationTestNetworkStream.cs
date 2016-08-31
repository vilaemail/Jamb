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
	/// Tests integration with .NET class NetworkStream. This covers testing WrappedNetworkStream, NetworkStreamFactory and NetworkHelper classes.
	/// Additionally it is testing assumptions made about NetworkStream in MessagePasser class.
	/// 
	/// NOTE: These tests use actual system network resources!
	/// </summary>
	[TestClass]
	public class IntegrationTestNetworkStream
	{
		private static readonly byte[] dummyMessage = { 1, 2, 3, 4, 5 };

		[TestCategory("Integration"), TestCategory("Longrunning"), TestMethod]
		public void BasicServerClient_ServerAndClientExchangeOneMessageAndCloseConnection_MessagesIntactGracefullExit()
		{
			byte[] buffer = new byte[100];
			int readBytes;
			var serverAndClientStream = GetServerAndClientStream();

			var serverStream = serverAndClientStream.Item1;
			var clientStream = serverAndClientStream.Item2;

			try
			{
				Assert.IsFalse(serverStream.DataAvailable, "We shouldn't have any data if we haven't sent anything");
				Assert.IsFalse(clientStream.DataAvailable, "We shouldn't have any data if we haven't sent anything");

				// Send one message as server, receive it as client and check it is preserved.
				serverStream.Write(dummyMessage, 0, dummyMessage.Length);
				Thread.Sleep(100);
				Assert.IsFalse(serverStream.DataAvailable, "We shouldn't have any data if we haven't sent anything");
				Assert.IsTrue(clientStream.DataAvailable, "We should have data because we have sent it");
				readBytes = clientStream.Read(buffer, 0, dummyMessage.Length / 2);
				Assert.AreEqual(dummyMessage.Length / 2, readBytes, "We should read exactly as much as we requested");
				Assert.IsFalse(serverStream.DataAvailable, "We shouldn't have any data if we haven't sent anything");
				Assert.IsTrue(clientStream.DataAvailable, "We should have more data remaining because we haven't read all of it");
				readBytes = clientStream.Read(buffer, dummyMessage.Length / 2, dummyMessage.Length - dummyMessage.Length / 2);
				Assert.AreEqual(dummyMessage.Length - dummyMessage.Length / 2, readBytes, "We should read exactly as much as we requested");
				Assert.IsFalse(serverStream.DataAvailable, "We shouldn't have any data if we haven't sent anything");
				Assert.IsFalse(clientStream.DataAvailable, "We shouldn't have any data because we have read all that was sent");
				for (int i = 0; i < dummyMessage.Length; i++)
				{
					Assert.AreEqual(dummyMessage[i], buffer[i], "We should receive same message that was sent. Index: " + i);
				}

				// Send messages at the same time from both sides
				serverStream.Write(dummyMessage, 0, dummyMessage.Length);
				clientStream.Write(dummyMessage, 0, dummyMessage.Length);
				Thread.Sleep(100);
				Assert.IsTrue(serverStream.DataAvailable, "We should have data because we have sent it");
				Assert.IsTrue(clientStream.DataAvailable, "We should have data because we have sent it");

				readBytes = clientStream.Read(buffer, 0, dummyMessage.Length);
				Assert.AreEqual(dummyMessage.Length, readBytes, "We should read exactly as much as we requested");
				Assert.IsFalse(clientStream.DataAvailable, "We shouldn't have any data because we have read all that was sent");
				for (int i = 0; i < dummyMessage.Length; i++)
				{
					Assert.AreEqual(dummyMessage[i], buffer[i], "We should receive same message that was sent. Index: " + i);
				}

				readBytes = serverStream.Read(buffer, 0, dummyMessage.Length);
				Assert.AreEqual(dummyMessage.Length, readBytes, "We should read exactly as much as we requested");
				Assert.IsFalse(serverStream.DataAvailable, "We shouldn't have any data because we have read all that was sent");
				for (int i = 0; i < dummyMessage.Length; i++)
				{
					Assert.AreEqual(dummyMessage[i], buffer[i], "We should receive same message that was sent. Index: " + i);
				}
			}
			finally
			{
				// Close connection from both ends
				clientStream?.Dispose();
				serverStream?.Dispose();
			}
		}

		[TestCategory("Integration"), TestCategory("Longrunning"), TestMethod]
		public void BasicServerClient_ConnectionIsTerminatedFromOneEnd_HandlesGracefully()
		{
			int readBytes;
			byte[] buffer = new byte[100];
			var serverAndClientStream = GetServerAndClientStream();
			var serverStream = serverAndClientStream.Item1;
			var clientStream = serverAndClientStream.Item2;

			try
			{
				Assert.IsFalse(serverStream.DataAvailable, "We shouldn't have any data if we haven't sent anything");
				Assert.IsFalse(clientStream.DataAvailable, "We shouldn't have any data if we haven't sent anything");

				// Send one message as server and close connection.
				// Receive it as client and check it is preserved.
				serverStream.Write(dummyMessage, 0, dummyMessage.Length);
				serverStream.Dispose();
				Thread.Sleep(100);
				Assert.IsTrue(clientStream.DataAvailable, "We should have data because we have sent it");

				readBytes = clientStream.Read(buffer, 0, dummyMessage.Length);
				Assert.AreEqual(dummyMessage.Length, readBytes, "We should read exactly as much as we requested");
				Assert.IsFalse(clientStream.DataAvailable, "We shouldn't have any data because we have read all that was sent");
				for (int i = 0; i < dummyMessage.Length; i++)
				{
					Assert.AreEqual(dummyMessage[i], buffer[i], "We should receive same message that was sent. Index: " + i);
				}
				Assert.IsFalse(clientStream.DataAvailable, "We shouldn't have any data because we have read all that was sent");

				// Make sure reads and writes behave as expected. Nothing to read and write passes as if everything is ok.
				readBytes = clientStream.Read(buffer, 0, dummyMessage.Length);
				Assert.AreEqual(0, readBytes, "We shouldn't read anything when connection is closed on other side");
				clientStream.Write(dummyMessage, 0, dummyMessage.Length);
			}
			finally
			{
				// Close connection from both ends
				clientStream?.Dispose();
				serverStream?.Dispose();
			}
		}

		[TestCategory("Integration"), TestCategory("Longrunning"), TestMethod]
		public void BasicServerClient_ReadAndWriteTimeout_ThrowsException()
		{
			byte[] buffer = new byte[100];
			var serverAndClientStream = GetServerAndClientStream();
			var serverStream = serverAndClientStream.Item1;
			var clientStream = serverAndClientStream.Item2;

			try
			{
				Assert.IsFalse(serverStream.DataAvailable, "We shouldn't have any data if we haven't sent anything");
				Assert.IsFalse(clientStream.DataAvailable, "We shouldn't have any data if we haven't sent anything");

				NetworkStream serverNetworkStream = typeof(WrappedNetworkStream).GetField("m_networkStream", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(serverStream) as NetworkStream;
				serverNetworkStream.ReadTimeout = 10;
				Exception thrownException = AssertHelper.AssertExceptionHappened(() => serverStream.Read(buffer, 0, buffer.Length), typeof(IOException), "We should get exception for read timeout.");
				Assert.AreEqual(typeof(SocketException), thrownException.InnerException.GetType(), "Inner exception should be socket exception");
				Assert.IsTrue(thrownException.InnerException.Message.Contains("A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond"), "Inner exception message should indicate timeout");
			}
			finally
			{
				// Close connection from both ends
				clientStream?.Dispose();
				serverStream?.Dispose();
			}
		}

		internal static Tuple<INetworkStream, INetworkStream> GetServerAndClientStream()
		{
			// Make sure all our blocking actions finish eventually
			CancellationTokenSource cts = new CancellationTokenSource();
			cts.CancelAfter(10000);

			// Get IP address so that we can create sockets
			List<IPAddress> addresses = NetworkHelper.GetLocalIPv4Addresses();
			Assert.IsTrue(addresses.Count > 0, "We must have network available for this test to work");
			IPAddress myIp = addresses[0];

			// We need factory to produce network streams
			var factory = new NetworkStreamFactory();

			// Try to establish connection
			Task<INetworkStream> serverStreamTask = Task.Run(() => factory.InstantiateForServer(myIp, 4959, cts.Token));
			Task<INetworkStream> clientStreamTask = Task.Run(() => factory.InstantiateForClient(myIp, 4959, cts.Token));
			Task.WhenAll(serverStreamTask, clientStreamTask).WaitAndThrowActualException();

			// Get the streams
			INetworkStream serverStream = null;
			INetworkStream clientStream = null;
			try
			{
				serverStream = serverStreamTask.Result;
				clientStream = clientStreamTask.Result;
			}
			catch(Exception)
			{
				serverStream?.Dispose();
				clientStream?.Dispose();
				throw;
			}

			return new Tuple<INetworkStream, INetworkStream>(serverStream, clientStream);
		}
	}
}
