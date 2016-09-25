namespace Jamb.Values
{
	/// <summary>
	/// Value that always returns default of TValue
	/// </summary>
	public class NullValue<TValue> : IValue<TValue>
	{
		/// <summary>
		/// Returns the default(TValue)
		/// </summary>
		public TValue Get()
		{
			return default(TValue);
		}
	}
}
