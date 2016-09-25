using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Logging
{
	/// <summary>
	/// Specifies importance of the specific log.
	/// </summary>
	public enum LogLevel
	{
		Debug = 5,
		Verbose = 10,
		Info = 15,
		Warning = 20,
		Error = 25,
		Off = 30
	}
}
