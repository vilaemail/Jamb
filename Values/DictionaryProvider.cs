using Jamb.Common;
using Jamb.Logging;
using System;
using System.Collections.Generic;

namespace Jamb.Values
{
	/// <summary>
	/// Provides values by storing them in memory using a dictionary.
	/// </summary>
	/// <typeparam name="TKey">Type of the dictionary key</typeparam>
	public class DictionaryProvider<TKey> : IValuesProvider<TKey>
	{
		private Dictionary<TKey, object> m_dictionary = new Dictionary<TKey, object>();

		/// <summary>
		/// Tries to retrieve value for the given key from dictionary. Throws ValueException if it fails.
		/// </summary>
		/// <typeparam name="TValue">Type of the value stored for the given key</typeparam>
		/// <param name="key">Key for which we will search for value</param>
		/// <returns>Value under the given key</returns>
		public TValue Get<TValue>(TKey key)
		{
			if(key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

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

		/// <summary>
		/// Sets the value for the given key.
		/// </summary>
		public void Set<TValue>(TKey key, TValue value)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			m_dictionary[key] = value;
		}
	}
}
