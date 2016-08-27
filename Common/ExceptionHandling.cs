using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jamb.Common
{
	/// <summary>
	/// Contains helper methods for exception handling.
	/// </summary>
	static class ExceptionHandling
	{
		/// <summary>
		/// Creates string containing all information available in the exception object.
		/// </summary>
		public static string CreateStringDescribingException(Exception e)
		{
			if (e == null)
			{
				throw new ArgumentNullException(nameof(e));
			}

			StringBuilder stringBuilder = new StringBuilder();
			CreateExceptionString(stringBuilder, e);

			return stringBuilder.ToString();
		}

		/// <summary>
		/// Helper method for appending info about exception to the given string builder. Recursivelly calls itself for all inner exceptions.
		/// </summary>
		/// <param name="stringBuilder">Builder to which we will append strings</param>
		/// <param name="e">Exception from which we are extracting information</param>
		/// <param name="indent">String containing the indentation prefix.</param>
		private static void CreateExceptionString(StringBuilder stringBuilder, Exception e, string indent = null)
		{
			Debug.Assert(stringBuilder != null);
			Debug.Assert(e != null);

			if (indent == null)
			{
				indent = string.Empty;
			}
			else if (indent.Length > 0)
			{
				stringBuilder.AppendFormat("{0}Inner ", indent);
			}

			stringBuilder.AppendFormat("Exception Found:\n{0}Type: {1}", indent, e.GetType().FullName);
			AppendMultiline(stringBuilder, "Message: ", e.Message ?? "", indent);
			AppendMultiline(stringBuilder, "Source: ", e.Source ?? "", indent);
			AppendMultiline(stringBuilder, "Stacktrace: ", e.StackTrace ?? "", indent);

			if (e.InnerException != null)
			{
				stringBuilder.Append("\n");
				CreateExceptionString(stringBuilder, e.InnerException, indent + "  ");
			}
		}

		/// <summary>
		/// Appends to the given string builder given text. If text has multiple lines each is appended separately.
		/// First new line is written, then indent string, then prefix and then one line of given text.
		/// </summary>
		private static void AppendMultiline(StringBuilder stringBuilder, string prefix, string text, string indent)
		{
			Debug.Assert(stringBuilder != null);
			Debug.Assert(prefix != null);
			Debug.Assert(text != null);
			Debug.Assert(indent != null);

			var messageLines = text.Split('\n');
			foreach (var messageLine in messageLines)
			{
				stringBuilder.AppendFormat("\n{0}{1}{2}", indent, prefix, messageLine);
			}
		}
	}
}
