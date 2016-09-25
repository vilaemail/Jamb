namespace Jamb.Values
{
	/// <summary>
	/// Value provider that does nothing. For the values always returns a default of TValue.
	/// </summary>
	public class NullValueProvider<TKey> : IValuesProvider<TKey>
	{
		/// <summary>
		/// Returns the default(TValue)
		/// </summary>
		public TValue Get<TValue>(TKey key)
		{
			return default(TValue);
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		public void Set<TValue>(TKey key, TValue value)
		{

		}
	}
}
