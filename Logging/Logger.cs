using System.Diagnostics;

namespace Jamb.Logging
{
	public static class Logger
	{
		private static ILogger m_impl = null;

		internal static void SetImplementation(ILogger implementation)
		{
			m_impl = implementation;
		}

		public static ILogger Instance => m_impl;
	}
}
