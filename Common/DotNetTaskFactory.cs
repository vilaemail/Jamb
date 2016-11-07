using System;
using System.Threading.Tasks;

namespace Jamb.Common
{
	/// <summary>
	/// Creates tasks that execute given delegates by calling the .NET Task.Factory
	/// </summary>
	public class DotNetTaskFactory : ITaskFactory
	{
		/// <summary>
		/// Calls the underlying implementation of Task.Factory.StartNew
		/// </summary>
		public Task StartNew(Action action)
		{
			return Task.Factory.StartNew(action);
		}
	}
}
