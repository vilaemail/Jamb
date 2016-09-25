using System;

namespace Jamb.Values
{
	/// <summary>
	/// Value implementation that retrieves its value from given provider with the given key.
	/// </summary>
	/// <typeparam name="TKey">Type of the key the provider stores</typeparam>
	/// <typeparam name="TValue">Type of the value this class returns</typeparam>
	public class ProviderBackedValue<TKey, TValue> : IValue<TValue>
	{
		private readonly TKey m_key;
		private readonly IValuesProvider<TKey> m_provider;

		/// <summary>
		/// Initializes class instance with provider from which value will be returned for the key given
		/// </summary>
		public ProviderBackedValue(IValuesProvider<TKey> valueProvider, TKey key)
		{
			if(valueProvider == null)
			{
				throw new ArgumentNullException(nameof(valueProvider));
			}

			m_provider = valueProvider;
			m_key = key;
		}

		/// <summary>
		/// Returns the value by calling underlying provider
		/// </summary>
		public TValue Get()
		{
			return m_provider.Get<TValue>(m_key);
		}
	}
}
