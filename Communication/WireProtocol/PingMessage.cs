using System.Runtime.Serialization;

namespace Jamb.Communication.WireProtocol
{
	/// <summary>
	/// Message used for unit testing purposes.
	/// </summary>
	[DataContract]
	internal class PingMessage : Message
	{
	}
}
