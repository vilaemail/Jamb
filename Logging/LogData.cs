using System;
using System.Collections.Generic;

namespace Jamb.Logging
{
	/// <summary>
	/// Dictionary containing data we would like to log together with the log message.
	/// </summary>
	[Serializable]
	public class LogData : Dictionary<string, string>
	{
	}
}
