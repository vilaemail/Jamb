using System;
using System.Diagnostics;
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
            if(e == null)
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
            stringBuilder.AppendFormat("\n{0}Message: {1}", indent, e.Message);
            stringBuilder.AppendFormat("\n{0}Source: {1}", indent, e.Source);
            stringBuilder.AppendFormat("\n{0}Stacktrace: {1}", indent, e.StackTrace);

            if (e.InnerException != null)
            {
                stringBuilder.Append("\n");
                CreateExceptionString(stringBuilder, e.InnerException, indent + "  ");
            }
        }
    }
}
