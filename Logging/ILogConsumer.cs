using System;

namespace Jamb.Logging
{
	/// <summary>
	/// Consumes log messages (i.e. persists it to disk or prints them in console).
	/// </summary>
	public interface ILogConsumer : IDisposable
	{
		/// <summary>
		/// Consume the given entry in the log.
		/// </summary>
		void AddLogEntry(string entry);
	}
}