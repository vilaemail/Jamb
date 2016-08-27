using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Jamb.Common
{
	/// <summary>
	/// Contains helper functions for dealing with Task objects.
	/// </summary>
	static class TaskHelper
	{
		/// <summary>
		/// Waits for task to complete.
		/// If task throws retrieves inner exception and throws it instead.
		/// </summary>
		public static void WaitAndThrowActualException(this Task task)
		{
			try
			{
				task.Wait();
			}
			catch (AggregateException e)
			{
				// Restore original exception state and throw
				ExceptionDispatchInfo.Capture(e.InnerException).Throw();
			}
		}

		/// <summary>
		/// Waits for task to complete and returns what the task returned.
		/// If task throws retrieves inner exception and throws it instead.
		/// </summary>
		public static T WaitAndThrowActualException<T>(this Task<T> task)
		{
			try
			{
				return task.Result;
			}
			catch (AggregateException e)
			{
				// Restore original exception state and throw
				ExceptionDispatchInfo.Capture(e.InnerException).Throw();
				throw; // Can't compile without this line
			}
		}
	}
}
