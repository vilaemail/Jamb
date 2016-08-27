using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Common
{
    static class TaskHelper
    {
        /// <summary>
		/// Waits for task to complete.
        /// If task throws gets inner exception and throws it.
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
        /// If task throws gets inner exception and throws it.
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
                throw; // Can't combile without this line
            }
        }
    }
}
