using System;
using System.Threading.Tasks;

namespace Jamb.Common
{
	/// <summary>
	/// Creates tasks that execute given delegates.
	/// </summary>
	public interface ITaskFactory
	{
		/// <summary>
		/// Creates and starts a task.
		/// </summary>
		Task StartNew(Action action);
	}
}
