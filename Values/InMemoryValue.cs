using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Values
{
	public class InMemoryValue<TValue> : IValue<TValue>
	{
		TValue m_value;

		public InMemoryValue(TValue value)
		{
			m_value = value;
		}

		public TValue Get()
		{
			return m_value;
		}

		public void Set(TValue newValue)
		{
			m_value = newValue;
		}
	}
}
