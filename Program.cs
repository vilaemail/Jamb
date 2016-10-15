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
		/// The main entry point for the application. Initializes logging and global handlers for exception and application exit.
		/// Shows main GUI element.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Initialize settings
			Values.Settings.SetImplementation(new Values.DotNetSettingsProvider<Values.SettingsKey>(Properties.Settings.Default));

			// Initialize logging
			Logging.Logger.SetImplementation(new Logging.LoggerFactory().CreateAsyncToFile(
#if DEBUG
				minimumLogLevel: Logging.LogLevel.Debug,
#else
				minimumLogLevel: Logging.LogLevel.Info,
#endif
				logFolder: Values.Settings.CurrentValue<string>(Values.SettingsKey.LogFolder),
				logFileNaming: Values.Settings.CurrentValue<string>(Values.SettingsKey.LogFileNameFormat),
				logPeriodInS: Values.Settings.Changable<int>(Values.SettingsKey.LogWrittingPeriod)));

			// Setup global event handlers
			Application.ApplicationExit += (object sender, EventArgs e) => ExitCleanup();
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

			// Start the application
			Logging.Logger.Instance.Log(Logging.LogLevel.Info, "Application initialization completed. Starting the application.");
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Menu());
		}

		/// <summary>
		/// Cleans up resources used by application. Called when application is exiting from the exit handler.
		/// </summary>
		internal static void ExitCleanup()
		{
			Logging.Logger.Instance?.Dispose();
		}

		private static bool s_handlingException = false;
		/// <summary>
		/// Handles an unhandled exception in application.
		/// 1. Tries to extract exception information and log it to the file. Informs user and exits application.
		/// 2. If 1 fails tries to show error message with why we failed in doing 1 and exits application.
		/// 3. If 2 fails just shows message that we failed and exits application.
		/// </summary>
		private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			if (s_handlingException)
			{
				// We got unhandled exception while we were handling unhandled exception - boy we are bad at programming...
				MessageBox.Show("Unfortunatelly application has stopped with an unrecoverable error. Additionally we failed to log the crash information. Please send logs to the developer. Exit code: 2.", "Unrecoverable error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(2);
			}
			s_handlingException = true;

			try
			{
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
				string outputInfoFile = string.Format("{0}\\{1}", Values.Settings.CurrentValue<string>(Values.SettingsKey.LogFolder), string.Format(Values.Settings.CurrentValue<string>(Values.SettingsKey.CrashFileNameFormat), DateTime.UtcNow));
				File.WriteAllText(outputInfoFile, exceptionDetails); //TODO: Make this path generation separate

				// Exit from application
				ExitCleanup();
				MessageBox.Show("Unfortunatelly application has stopped with an unrecoverable error. Please send logs and crash information to the developer. Exit code: 1.", "Unrecoverable error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(1);
			}
			catch(Exception ex)
			{
				MessageBox.Show("Unfortunatelly application has stopped with an unrecoverable error. Please send logs and crash information to the developer. Exit code: 3.\nException info:\n" + ExceptionHandling.CreateStringDescribingException(ex), "Unrecoverable error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(3);
			}
		}
	}
}
