using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Values
{
	public class ProviderBackedValue<TKey, TValue> : IValue<TValue>
	{
		TKey m_key;
		IValuesProvider<TKey> m_provider;

		public ProviderBackedValue(IValuesProvider<TKey> valueProvider, TKey key)
		{
			m_provider = valueProvider;
			m_key = key;
		}
		public TValue Get()
		{
			return m_provider.Get<TValue>(m_key);
		}
	}
}
