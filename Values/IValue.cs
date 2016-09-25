namespace Jamb.Values
{
	/// <summary>
	/// Provides a value of specific type TValue
	/// </summary>
	public interface IValue<TValue>
	{
		/// <summary>
		/// Returns the value.
		/// </summary>
		TValue Get();
	}
}
