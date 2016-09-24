using Jamb.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jamb
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Initialize logging
#if DEBUG
			Logging.Logger.SetImplementation(new Logging.LoggerFactory().CreateAsyncToFile(Logging.LogLevel.Debug, "logs"));
#else
			Logging.Logger.SetImplementation(new Logging.LoggerFactory().CreateAsyncToFile(Logging.LogLevel.Info, "logs"));
#endif
			// Setup global event handlers
			Application.ApplicationExit += (object sender, EventArgs e) => ExitCleanup();
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

			// Start the application
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Menu());
		}

		private static void ExitCleanup()
		{
			Logging.Logger.Instance?.Dispose();
		}

		private static bool s_handlingException = false;
		private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			if (s_handlingException)
			{
				// We got unhandled exception while we were handling unhandled exception - boy we are bad at programming...
				Environment.Exit(2);
			}
			s_handlingException = true;

			// Get exception details
			string exceptionDetails = null;
			if (e.ExceptionObject is Exception)
			{
				exceptionDetails = ExceptionHandling.CreateStringDescribingException(e.ExceptionObject as Exception);
			}
			else
			{
				exceptionDetails = e.ExceptionObject.ToString();
			}

			// Log exception details in separate file
			File.WriteAllText("logs\\crash.log", exceptionDetails); //TODO: Make this path generation separate

			// Exit from application
			ExitCleanup();
			Environment.Exit(1);
		}
	}
}
