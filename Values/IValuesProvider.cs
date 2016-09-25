using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Values
{
	public interface IValuesProvider<TKey>
	{
		TValue Get<TValue>(TKey key);
		void Set<TValue>(TKey key, TValue value);
	}
}
