using Jamb.Common;
using Jamb.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Values
{
	public class DictionaryProvider<TKey> : IValuesProvider<TKey>
	{
		private Dictionary<TKey, object> m_dictionary = new Dictionary<TKey, object>();

		public TValue Get<TValue>(TKey key)
		{
			object value;
			if (!m_dictionary.TryGetValue(key, out value))
			{
				throw new ValuesException("Can't retrieve value with the given key.");
			}

			// We don't expect to have invalid keys in application once shipped, using try catch will improve performance.
			try
			{
				return (TValue)value;
			}
			catch (InvalidCastException e)
			{
				Logger.Instance.Log(LogLevel.Error, "Value not of expected type.", () => new LogData()
				{
					{ "Key", key.ToString() },
					{ "Value", value.ToString() },
					{ "ValueType", value.GetType().ToString() },
					{ "ExpectedValueType", typeof(TValue).ToString() },
					{ "Exception", ExceptionHandling.CreateStringDescribingException(e) }
				});
				throw new ValuesException("Value is not of the expected type.", e);
			}
		}

		public void Set<TValue>(TKey key, TValue value)
		{
			m_dictionary[key] = value;
		}
	}
}
