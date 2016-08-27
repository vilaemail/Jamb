using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Communication.WireProtocol
{
	[DataContract]
	abstract class Message
	{
		public static IEnumerable<Type> KnownTypes { get; private set; }
		static Message()
		{
			var myType = typeof(Message);
			var typesThatImplement = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => myType.IsAssignableFrom(p));
			KnownTypes = typesThatImplement;
		}
	}
}
