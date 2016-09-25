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
	}
}
