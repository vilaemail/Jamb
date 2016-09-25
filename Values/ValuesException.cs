using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Values
{
	[Serializable]
	public class ValuesException : Exception
	{
		internal ValuesException()
		{
		}

		internal ValuesException(string message)
			: base(message)
		{
		}

		internal ValuesException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
