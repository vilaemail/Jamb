namespace Jamb.Values
{
	/// <summary>
	/// Value that is stored in memory and initialized using constructor.
	/// It is possible to change the value using the Set method.
	/// </summary>
	public class InMemoryValue<TValue> : IValue<TValue>
	{
		private TValue m_value;

		/// <summary>
		/// Initializes class instance with the given value.
		/// </summary>
		public InMemoryValue(TValue value)
		{
			m_value = value;
		}

		/// <summary>
		/// Returns value that is stored in this instance
		/// </summary>
		public TValue Get()
		{
			return m_value;
		}

		/// <summary>
		/// Changes the value this class returns.
		/// </summary>
		public void Set(TValue newValue)
		{
			m_value = newValue;
		}
	}
}
