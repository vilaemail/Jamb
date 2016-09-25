using System;

namespace Jamb.Values
{
	/// <summary>
	/// Thrown if get or set of a value fails
	/// </summary>
	[Serializable]
	public class ValuesException : Exception
	{
		internal ValuesException()
		{
		}

		internal ValuesException(string message)
			: base(message)
		{
		}

		internal ValuesException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
