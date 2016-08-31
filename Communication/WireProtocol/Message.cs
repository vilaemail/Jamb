using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Jamb.Communication.WireProtocol
{
	[DataContract]
	internal abstract class Message
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
