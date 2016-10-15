namespace Jamb.Values
{
	/// <summary>
	///  Static class used for receiving application level instance of IValuesProvider for settings.
	/// </summary>
	public class Settings
	{
		private static IValuesProvider<SettingsKey> m_impl;

		/// <summary>
		/// Changes the implementation of settings provider used by the application.
		/// </summary>
		public static void SetImplementation(IValuesProvider<SettingsKey> implementation)
		{
			m_impl = implementation;
		}

		/// <summary>
		/// Instance of settings provider that should be used by the application.
		/// </summary>
		public static IValuesProvider<SettingsKey> Instance
		{
			get
			{
				if (m_impl == null)
				{
					m_impl = new NullValueProvider<SettingsKey>();
				}

				return m_impl;
			}
		}

		/// <summary>
		/// Returns a IValue that changes when setting changes.
		/// </summary>
		/// <typeparam name="TValue">Type of setting value</typeparam>
		/// <param name="key">Key of the setting</param>
		public static IValue<TValue> Changable<TValue>(SettingsKey key)
		{
			return new ProviderBackedValue<SettingsKey, TValue>(Instance, key);
		}

		/// <summary>
		/// Returns a IValue that returns the current setting. If setting changes in the future it
		/// will not be reflected by IValue
		/// </summary>
		/// <typeparam name="TValue">Type of setting value</typeparam>
		/// <param name="key">Key of the setting</param>
		public static IValue<TValue> FromCurrentValue<TValue>(SettingsKey key)
		{
			return new InMemoryValue<TValue>(Instance.Get<TValue>(key));
		}

		/// <summary>
		/// Returns the current value of the setting
		/// </summary>
		/// <typeparam name="TValue">Type of setting value</typeparam>
		/// <param name="key">Key of the setting</param>
		public static TValue CurrentValue<TValue>(SettingsKey key)
		{
			return Instance.Get<TValue>(key);
		}
	}
}
