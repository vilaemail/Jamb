using Jamb.Common;
using Jamb.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Values
{
	public class DotNetSettingsProvider<TKey> : IValuesProvider<TKey>
	{
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

		public void Set<TValue>(TKey key, TValue value)
		{
			if (!SaveSetting(key, value))
			{
				throw new ValuesException("Failed to save setting.");
			}
		}

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
