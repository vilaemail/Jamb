using System;

namespace Jamb.Logging
{
	public interface ILogConsumer : IDisposable
	{
		void AddLogEntry(string entry);
	}
}