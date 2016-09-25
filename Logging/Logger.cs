using System.Diagnostics;

namespace Jamb.Logging
{
	/// <summary>
	/// Static class used for receiving application level instance of ILogger.
	/// When retrieving the instance if it is set to null, automatically sets it to NullLoggerThatEvaluatesLogData.
	/// </summary>
	public static class Logger
	{
		private static ILogger m_impl;

		/// <summary>
		/// Changes the implementation of logger used by the application.
		/// </summary>
		public static void SetImplementation(ILogger implementation)
		{
			m_impl = implementation;
		}

		/// <summary>
		/// Instance of logger that should be used by the application.
		/// </summary>
		public static ILogger Instance
		{
			get
			{
				if (m_impl == null)
				{
					m_impl = new NullLoggerThatEvaluatesLogData();
				}

				return m_impl;
			}
		}
	}
}
