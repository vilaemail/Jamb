using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Values
{
	public class Settings
	{
		private static IValuesProvider<SettingsKey> m_impl = new NullValueProvider<SettingsKey>();

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
		public static IValuesProvider<SettingsKey> Instance => m_impl;
	}
}
