using Jamb.Common;
using Jamb.Logging;
using System;

namespace Jamb.Values
{
	/// <summary>
	/// Returns values by translating TKey to string and using that as a key to .NET provided settings.
	/// </summary>
	public class DotNetSettingsProvider<TKey> : IValuesProvider<TKey>
	{
		/// <summary>
		/// Tries to get the value from .NET settings. If it fails ValueException is thrown.
		/// </summary>
		/// <typeparam name="TValue">Type of which returned value should be</typeparam>
		/// <param name="key">Key that will be converted to string and used as a key in .NET settings</param>
		/// <returns>The value retrieved from .NET settings and casted to TValue</returns>
		public TValue Get<TValue>(TKey key)
		{
			object value;
			if (!RetrieveSetting(key, out value))
			{
				throw new ValuesException("Can't retrieve setting with the given key.");
			}

			// We don't expect to have invalid keys in application once shipped, using try catch will improve performance.
			try
			{
				return (TValue)value;
			}
			catch (InvalidCastException e)
			{
				Logger.Instance.Log(LogLevel.Error, "Setting not of expected type.", () => new LogData()
				{
					{ "Key", key.ToString() },
					{ "Value", value.ToString() },
					{ "ValueType", value.GetType().ToString() },
					{ "ExpectedValueType", typeof(TValue).ToString() },
					{ "Exception", ExceptionHandling.CreateStringDescribingException(e) }
				});
				throw new ValuesException("Setting is not of the expected type.", e);
			}
		}

		/// <summary>
		/// Tries to set the value in .NET settings. If it fails ValueException is thrown.
		/// </summary>
		/// <typeparam name="TValue">The type of which is the value</typeparam>
		/// <param name="key">Key that will be converted to string and used as a key in .NET settings</param>
		/// <param name="value">Value to be set for the given key</param>
		public void Set<TValue>(TKey key, TValue value)
		{
			if (!SaveSetting(key, value))
			{
				throw new ValuesException("Failed to save setting.");
			}
		}

		/// <summary>
		/// Tries to retrieve the setting from .NET settings.
		/// </summary>
		/// <param name="key">Key for which we are looking for value</param>
		/// <param name="value">Object retrieved from .NET settings if successful</param>
		/// <returns>Whether or not it was successful</returns>
		private bool RetrieveSetting(TKey key, out object value)
		{
			try
			{
				value = Properties.Settings.Default[key.ToString()];
				return true;
			}
			catch (Exception e)
			{
				Logger.Instance.Log(LogLevel.Error, "Failed to retrieve setting.", () => new LogData()
				{
					{ "Key", key.ToString() },
					{ "Exception", ExceptionHandling.CreateStringDescribingException(e) }
				});
				value = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to save the setting under the given key in .NET settings.
		/// </summary>
		/// <typeparam name="TValue">Type of the value</typeparam>
		/// <param name="key">Key for which we will set the value</param>
		/// <param name="value">Value to be set</param>
		/// <returns>Whether or not it was successful</returns>
		private bool SaveSetting<TValue>(TKey key, TValue value)
		{
			try
			{
				Properties.Settings.Default[key.ToString()] = value;
				return true;
			}
			catch (Exception e)
			{
				Logger.Instance.Log(LogLevel.Error, "Failed to save setting.", () => new LogData()
				{
					{ "Key", key.ToString() },
					{ "Value", value.ToString() },
					{ "Exception", ExceptionHandling.CreateStringDescribingException(e) }
				});
				return false;
			}
		}
	}
}
