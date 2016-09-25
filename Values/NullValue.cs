using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Values
{
	public class NullValue<TValue> : IValue<TValue>
	{
		public TValue Get()
		{
			return default(TValue);
		}
	}
}
