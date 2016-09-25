namespace Jamb.Values
{
	/// <summary>
	/// Provides/sets values of different type indexed by keys of type TKey.
	/// </summary>
	public interface IValuesProvider<TKey>
	{
		/// <summary>
		/// Returns the value of type TValue that is tied to the given key.
		/// </summary>
		TValue Get<TValue>(TKey key);

		/// <summary>
		/// Sets the given key value to the given value.
		/// </summary>
		void Set<TValue>(TKey key, TValue value);
	}
}
