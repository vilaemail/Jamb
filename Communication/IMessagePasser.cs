using System.Threading;
using Jamb.Communication.WireProtocol;
using System;

namespace Jamb.Communication
{
	internal interface IMessagePasser : IDisposable
	{
		/// <summary>
		/// Synchronously tries to receive a message.
		/// Throws OperationCanceledException on cancelation, otherwise CommunicationException.
		/// </summary>
		Message ReceiveMessage(CancellationToken cancelToken);
		
		/// <summary>
		/// Synchronously tries to send a message.
		/// Throws OperationCanceledException on cancelation, otherwise CommunicationException.
		/// </summary>
		void SendMessage(Message messageToSend, CancellationToken cancelToken);
	}
}