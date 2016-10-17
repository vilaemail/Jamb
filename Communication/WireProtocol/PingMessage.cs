using System.Runtime.Serialization;

namespace Jamb.Communication.WireProtocol
{
	/// <summary>
	/// This message is sent when we don't have actual messages, but want to let know the other party
	/// that we are still available.
	/// </summary>
	[DataContract]
	internal class PingMessage : Message
	{
	}
}
