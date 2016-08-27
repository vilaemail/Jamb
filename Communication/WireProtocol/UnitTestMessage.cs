using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Communication.WireProtocol
{
	/// <summary>
	/// Message used for unit testing purposes.
	/// </summary>
	[DataContract]
	internal class UnitTestMessage : Message
	{
		[DataMember]
		public int IntDataMember { get; set; }

		[DataMember]
		public string StringDataMember { get; set; }

		[DataMember]
		public List<string> ListDataMember { get; set; }

		[DataContract]
		internal class NestedUnitTestDataContract
		{
			[DataMember]
			public int IntDataMember { get; set; }
		}

		[DataMember]
		public NestedUnitTestDataContract CustomObjectDataMember { get; set; }

		public int IntNonDataMemeber { get; set; }
	}
}
