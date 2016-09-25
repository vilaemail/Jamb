using System.Diagnostics;

namespace Jamb.Logging
{
	/// <summary>
	/// Static class used for receiving application level instance of ILogger.
	/// </summary>
	public static class Logger
	{
		private static ILogger m_impl = new NullLogger();

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
		public static ILogger Instance => m_impl;
	}
}
