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
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JambTests.Communication
{
	[TestClass]
	public class UnitTestMessagePasser
	{
		private static readonly byte[] s_normalMessage = { 0, 0, 0, 170, 1, 123, 34, 95, 95, 116, 121, 112, 101, 34, 58, 34, 85, 110, 105, 116, 84, 101, 115, 116, 77, 101, 115, 115, 97, 103, 101, 58, 35, 74, 97, 109, 98, 46, 67, 111, 109, 109, 117, 110, 105, 99, 97, 116, 105, 111, 110, 46, 87, 105, 114, 101, 80, 114, 111, 116, 111, 99, 111, 108, 34, 44, 34, 67, 117, 115, 116, 111, 109, 79, 98, 106, 101, 99, 116, 68, 97, 116, 97, 77, 101, 109, 98, 101, 114, 34, 58, 110, 117, 108, 108, 44, 34, 73, 110, 116, 68, 97, 116, 97, 77, 101, 109, 98, 101, 114, 34, 58, 49, 53, 44, 34, 76, 105, 115, 116, 68, 97, 116, 97, 77, 101, 109, 98, 101, 114, 34, 58, 91, 34, 97, 98, 99, 34, 44, 34, 120, 121, 122, 49, 48, 34, 93, 44, 34, 83, 116, 114, 105, 110, 103, 68, 97, 116, 97, 77, 101, 109, 98, 101, 114, 34, 58, 34, 115, 116, 114, 49, 48, 34, 125 };

		[TestMethod]
		public void SendMessage_NullMessage_ThrowsException()
		{
			Mock<INetworkStream> mockStream = new Mock<INetworkStream>(MockBehavior.Strict);
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.SendMessage(null, new CancellationToken()), typeof(ArgumentNullException), "We should thrown on null argument");
		}

		[TestMethod]
		public void SendMessage_UnexpectedNetworkStreamException_ExceptionThrown()
		{
			Mock<INetworkStream> mockStream = new Mock<INetworkStream>(MockBehavior.Strict);
			mockStream.Setup(obj => obj.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new ApplicationException("Unit test exception"));
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.SendMessage(CreateNormalMessage(10), new CancellationToken()), typeof(UnknownCommunicationException), "We should throw on unexpected exception");
		}

		[TestMethod]
		public void SendMessage_NormalMessage_MessageWrittenToStreamAsExpected()
		{
			SendMessage_NormalMessageWithSpecifiedNetworkDelay_MessageWrittenToStreamAsExpected(0);
		}

		[TestCategory("Longrunning"), TestMethod]
		public void SendMessage_NormalMessageWithNetworkDelays_MessageWrittenToStreamAsExpected()
		{
			SendMessage_NormalMessageWithSpecifiedNetworkDelay_MessageWrittenToStreamAsExpected(100);
		}

		[TestCategory("Longrunning"), TestMethod]
		public void SendMessage_NormalMessageWithCancelationRequested_CancelationExceptionThrown()
		{
			int expectedSize = 5;
			byte[] expectedHeader = { 0, 0, 0, 170, 1 };

			SendMessage_NormalMessageWithSpecifiedNetworkDelay_MessageWrittenToStreamAsExpected(500, (underTest, writeBuffer, mockStream) =>
			{
				// Send the message
				CancellationTokenSource cts = new CancellationTokenSource();
				cts.CancelAfter(TimeSpan.FromMilliseconds(250));
				AssertHelper.AssertExceptionHappened(() => underTest.SendMessage(CreateNormalMessage(10), cts.Token), typeof(OperationCanceledException), "We should raise exception when we are canceled");

				// Assert we only sent header
				byte[] sentData = writeBuffer.WrittenData;
				Assert.AreEqual(expectedSize, sentData.Length, "We should have sent expected amount of data");
				AssertHeader(expectedHeader, sentData);
				mockStream.Verify(obj => obj.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
			});
		}

		private static void SendMessage_NormalMessageWithSpecifiedNetworkDelay_MessageWrittenToStreamAsExpected(int delay, Action<MessagePasser, WriteNetworkStreamBuffer, Mock<INetworkStream>> sendAndAssertCode = null)
		{
			int expectedSize = 175;
			byte[] expectedHeader = { 0, 0, 0, 170, 1 };
			// Setup mock network stream that simply writes to buffer
			WriteNetworkStreamBuffer writeBuffer = new WriteNetworkStreamBuffer(delay);
			Mock<INetworkStream> mockStream = SetupWriteOnlyNetworkStream(writeBuffer);
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			if (sendAndAssertCode != null)
			{
				sendAndAssertCode(underTest, writeBuffer, mockStream);
				return;
			}

			// Send the message
			underTest.SendMessage(CreateNormalMessage(10), new CancellationToken());

			// Assert that message was sent
			mockStream.Verify(obj => obj.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));

			byte[] sentData = writeBuffer.WrittenData;
			// Assert
			Assert.AreEqual(expectedSize, sentData.Length, "We should have sent expected amount of data");
			AssertHeader(expectedHeader, sentData);
			// Assert contents
			UnitTestMessage sentMessage = DeserializeMessage<UnitTestMessage>(sentData.Skip(5).ToArray());
			AssertNormalMessage(sentMessage, 10);
		}

		[TestMethod]
		public void ReceiveMessage_UnexpectedNetworkStreamException_ExceptionThrown()
		{
			Mock<INetworkStream> mockStream = new Mock<INetworkStream>(MockBehavior.Strict);
			mockStream.Setup(obj => obj.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new ApplicationException("Unit test exception"));
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.ReceiveMessage(new CancellationToken()), typeof(UnknownCommunicationException), "We should throw on unexpected exception");
		}

		[TestMethod]
		public void ReceiveMessage_NotSupportedProtocol_ThrowsException()
		{
			byte[] message = (byte[])s_normalMessage.Clone();
			message[4]++; // Change protocol version
			ReadNetworkStreamBuffer readBuffer = new ReadNetworkStreamBuffer(message);
			Mock<INetworkStream> mockStream = SetupReadOnlyNetworkStream(readBuffer);
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.ReceiveMessage(new CancellationToken()), typeof(ProtocolException), "We should throw on unsupported protocol", "Unsupported protocol version");
		}

		[TestMethod]
		public void ReceiveMessage_TooBigMessage_ThrowsException()
		{
			byte[] message = (byte[])s_normalMessage.Clone();
			message[0] = 100; // Modify header such that message is huge
			ReadNetworkStreamBuffer readBuffer = new ReadNetworkStreamBuffer(message);
			Mock<INetworkStream> mockStream = SetupReadOnlyNetworkStream(readBuffer);
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.ReceiveMessage(new CancellationToken()), typeof(ProtocolException), "We should throw on too large message", "Message to be received is too large");
		}

		[TestMethod]
		public void ReceiveMessage_MalformedMessagePayload_ThrowsException()
		{
			byte[] message = (byte[])s_normalMessage.Clone();
			message[100] = 0; // Change message contents. This should prevent us from deserializing the message.
			ReadNetworkStreamBuffer readBuffer = new ReadNetworkStreamBuffer(message);
			Mock<INetworkStream> mockStream = SetupReadOnlyNetworkStream(readBuffer);
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			AssertHelper.AssertExceptionHappened(() => underTest.ReceiveMessage(new CancellationToken()), typeof(MalformedMessageException), "We should throw on malformed message");
		}

		[TestMethod]
		public void ReceiveMessage_NormalMessage_MessageReadFromStreamAsExpected()
		{
			byte[] message = (byte[])s_normalMessage.Clone();
			ReadNetworkStreamBuffer readBuffer = new ReadNetworkStreamBuffer(message);
			Mock<INetworkStream> mockStream = SetupReadOnlyNetworkStream(readBuffer);
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			UnitTestMessage recivedMessage = underTest.ReceiveMessage(new CancellationToken()) as UnitTestMessage;
			AssertNormalMessage(recivedMessage, 10);
		}

		[TestMethod]
		public void ReceiveMessage_NormalMessageWithNetworkDelays_MessageReadFromStreamAsExpected()
		{
			byte[] message = (byte[])s_normalMessage.Clone();
			ReadNetworkStreamBuffer readBuffer = new ReadNetworkStreamBuffer(message, 500);
			Mock<INetworkStream> mockStream = SetupReadOnlyNetworkStream(readBuffer);
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			UnitTestMessage recivedMessage = underTest.ReceiveMessage(new CancellationToken()) as UnitTestMessage;
			AssertNormalMessage(recivedMessage, 10);
		}

		[TestCategory("Longrunning"), TestMethod]
		public void ReceiveMessage_NormalMessageWithCancelationRequested_CancelationExceptionThrown()
		{
			byte[] message = (byte[])s_normalMessage.Clone();
			ReadNetworkStreamBuffer readBuffer = new ReadNetworkStreamBuffer(message, 500);
			Mock<INetworkStream> mockStream = SetupReadOnlyNetworkStream(readBuffer);
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			CancellationTokenSource cts = new CancellationTokenSource();
			cts.CancelAfter(250);
			AssertHelper.AssertExceptionHappened(() => underTest.ReceiveMessage(cts.Token), typeof(OperationCanceledException), "We should throw when cancelation is requested");
			mockStream.Verify(obj => obj.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
			Assert.AreEqual(message.Length - 5, readBuffer.RemainingBytes, "We should have read only header");
		}

		[TestCategory("Longrunning"), TestMethod]
		public void ReceiveMessage_NormalMessageWhenSomeBytesAreLate_MessageIsReceivedAsExpected()
		{
			// Set up test in such a way that we don't send whole message when read method is called
			byte[] message = (byte[])s_normalMessage.Clone();
			ReadNetworkStreamBuffer readBuffer = new ReadNetworkStreamBuffer(message, 250, true);
			Mock<INetworkStream> mockStream = SetupReadOnlyNetworkStream(readBuffer);
			MessagePasser underTest = ConstructWithDefaultSettings(mockStream.Object);

			// Run the receiving on another thread and wait to be sure the task is retrying
			Task<UnitTestMessage> recivedMessageTask = Task.Run(() => underTest.ReceiveMessage(new CancellationToken()) as UnitTestMessage);
			Thread.Sleep(1500);
			Assert.IsFalse(recivedMessageTask.IsCompleted, "We shouldn't complete until we receive the message");

			// Send the remaining byte
			readBuffer.HideOnePart = false;
			UnitTestMessage recivedMessage = recivedMessageTask.Result;
			// Assert we have deserialized as expected
			AssertNormalMessage(recivedMessage, 10);
		}

		private static Mock<INetworkStream> SetupWriteOnlyNetworkStream(WriteNetworkStreamBuffer writeBuffer)
		{
			Mock<INetworkStream> mockStream = new Mock<INetworkStream>(MockBehavior.Strict);
			mockStream.Setup(obj => obj.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback<byte[], int, int>((buffer, offset, size) =>
			{
				writeBuffer.Write(buffer.Skip(offset).Take(size).ToArray<byte>());
			});
			return mockStream;
		}

		private static Mock<INetworkStream> SetupReadOnlyNetworkStream(ReadNetworkStreamBuffer readBuffer)
		{
			Mock<INetworkStream> mockStream = new Mock<INetworkStream>(MockBehavior.Strict);
			mockStream.Setup(obj => obj.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback<byte[], int, int>((buffer, offset, size) =>
			{
				if (readBuffer.RemainingBytes >= size)
				{
					readBuffer.Read(size).CopyTo(buffer, offset);
				}
				else
				{
					readBuffer.Read(readBuffer.RemainingBytes).CopyTo(buffer, offset);
				}
			}).Returns(() =>
			{
				return readBuffer.NumberOfBytesReadLastTime;
			});
			mockStream.SetupGet(obj => obj.DataAvailable).Returns(() => readBuffer.RemainingBytes > 0);
			return mockStream;
		}

		internal static UnitTestMessage CreateNormalMessage(int id = 0)
		{
			return new UnitTestMessage()
			{
				IntDataMember = 5 + id,
				IntNonDataMemeber = 10,
				ListDataMember = new List<string> { "abc", "xyz" + id },
				StringDataMember = "str" + id,
				CustomObjectDataMember = null
			};
		}

		internal static UnitTestMessage CreateNestedMessage(int id = 0)
		{
			return new UnitTestMessage()
			{
				IntDataMember = 5 + id,
				IntNonDataMemeber = 10,
				ListDataMember = new List<string> { "abc", "xyz" + id },
				StringDataMember = "str" + id,
				CustomObjectDataMember = new UnitTestMessage.NestedUnitTestDataContract() {IntDataMember = id * 2}
			};
		}

		private static T DeserializeMessage<T>(byte[] data)
		{
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				return (T)serializer.ReadObject(memoryStream);
			}
		}

		private static void AssertHeader(byte[] expected, byte[] actual)
		{
			for (int i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actual[i], "Header should be as expected. Position: " + i);
			}
		}

		internal static void AssertNormalMessage(UnitTestMessage message, int expectedId)
		{
			UnitTestMessage expectedMessage = CreateNormalMessage(expectedId);

			Assert.AreEqual(expectedMessage.IntDataMember, message.IntDataMember, "Int data memeber should be the same");
			Assert.AreEqual(expectedMessage.ListDataMember[0], message.ListDataMember[0], "List data memeber should be the same. Item 0");
			Assert.AreEqual(expectedMessage.ListDataMember[1], message.ListDataMember[1], "List data memeber should be the same. Item 1");
			Assert.AreEqual(expectedMessage.StringDataMember, message.StringDataMember, "String data memeber should be the same");
			Assert.AreEqual(expectedMessage.CustomObjectDataMember, message.CustomObjectDataMember, "CustomObject data memeber should be the same");
			Assert.AreEqual(0, message.IntNonDataMemeber, "Int that is not data memeber should not be the same");
		}

		internal static void AssertNestedMessage(UnitTestMessage message, int expectedId)
		{
			UnitTestMessage expectedMessage = CreateNestedMessage(expectedId);

			Assert.AreEqual(expectedMessage.IntDataMember, message.IntDataMember, "Int data memeber should be the same");
			Assert.AreEqual(expectedMessage.ListDataMember[0], message.ListDataMember[0], "List data memeber should be the same. Item 0");
			Assert.AreEqual(expectedMessage.ListDataMember[1], message.ListDataMember[1], "List data memeber should be the same. Item 1");
			Assert.AreEqual(expectedMessage.StringDataMember, message.StringDataMember, "String data memeber should be the same");
			Assert.AreEqual(expectedMessage.CustomObjectDataMember.IntDataMember, message.CustomObjectDataMember.IntDataMember, "CustomObject data memeber should be the same");
			Assert.AreEqual(0, message.IntNonDataMemeber, "Int that is not data memeber should not be the same");
		}

		private static MessagePasser ConstructWithDefaultSettings(INetworkStream stream)
		{
			return new MessagePasser(stream, InMemoryValue<int>.Is(10240), InMemoryValue<int>.Is(500));
		}

		/// <summary>
		/// Helper class to which we can write as if we are writting to the stream.
		/// </summary>
		private class WriteNetworkStreamBuffer
		{
			private const int c_networkBufferSize = 1024;
			private byte[] m_networkBuffer = new byte[c_networkBufferSize];
			private int m_networkBufferEmptyPosition = 0;
			private int m_networkLag;
			public WriteNetworkStreamBuffer(int networkLagInMs = 0)
			{
				m_networkLag = networkLagInMs;
			}

			/// <summary>
			/// Writes whole array to the buffer
			/// </summary>
			public void Write(byte[] data)
			{
				Assert.IsTrue(m_networkBufferEmptyPosition + data.Length <= c_networkBufferSize, "Not enough space in unit test buffer");
				data.CopyTo(m_networkBuffer, m_networkBufferEmptyPosition);
				m_networkBufferEmptyPosition += data.Length;
				if (m_networkLag > 0)
				{
					Thread.Sleep(m_networkLag);
				}
			}

			/// <summary>
			/// Get copy of data that is written to the buffer since it was created
			/// </summary>
			public byte[] WrittenData => m_networkBuffer.Take(m_networkBufferEmptyPosition).ToArray();
		}

		/// <summary>
		/// Helper class from which we can read as if we are reading from the stream.
		/// </summary>
		private class ReadNetworkStreamBuffer
		{
			private byte[] m_networkBuffer;
			private int m_networkBufferCurrentPosition = 0;
			private int m_networkLag;

			/// <summary>
			/// Whether or not one part of the message is hidden, as if not received yet
			/// </summary>
			public bool HideOnePart { get; set; }
			/// <summary>
			/// Number of bytes there is to be read
			/// </summary>
			public int RemainingBytes => Math.Max(0, m_networkBuffer.Length - m_networkBufferCurrentPosition - (HideOnePart ? 1 : 0));

			/// <summary>
			/// Number of bytes that were read during last read operation
			/// </summary>
			public int NumberOfBytesReadLastTime { get; private set; } = 0;

			public ReadNetworkStreamBuffer(byte[] contents, int networkLag = 0, bool hideOnePart = false)
			{
				m_networkBuffer = contents;
				m_networkLag = networkLag;
				HideOnePart = hideOnePart;
			}

			/// <summary>
			/// Writes whole array to the buffer
			/// </summary>
			public byte[] Read(int count)
			{
				Assert.IsTrue(count + m_networkBufferCurrentPosition <= m_networkBuffer.Length, "Requested more data than we have");
				if (m_networkLag > 0)
				{
					Thread.Sleep(m_networkLag);
				}
				byte[] returnValue = m_networkBuffer.Skip(m_networkBufferCurrentPosition).Take(count).ToArray();
				m_networkBufferCurrentPosition += count;
				NumberOfBytesReadLastTime = count;
				return returnValue;
			}
		}
	}
}
