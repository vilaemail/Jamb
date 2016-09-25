using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Values
{
	public class NullValueProvider<TKey> : IValuesProvider<TKey>
	{
		public TValue Get<TValue>(TKey key)
		{
			return default(TValue);
		}

		public void Set<TValue>(TKey key, TValue value)
		{

		}
	}
}
