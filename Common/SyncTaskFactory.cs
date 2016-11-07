using System;
using System.Threading.Tasks;

namespace Jamb.Common
{
	/// <summary>
	/// Executes actions synchronously and returns Task.FromResult of results.
	/// </summary>
	public class SyncTaskFactory : ITaskFactory
	{
		/// <summary>
		/// Executes action synchronously
		/// </summary>
		public Task StartNew(Action action)
		{
			action();
			return Task.FromResult(false);
		}
	}
}
