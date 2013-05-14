using System;
using System.IO;
using System.Text;

namespace FileServer.Infraestructure
{
    public static class Logger
    {
        public static bool AppendInner(StringBuilder sb, Exception e)
        {
            if (e.InnerException != null)
            {
                sb.AppendLine(e.InnerException.Message);
                return AppendInner(sb, e.InnerException);
            }
            return false;
        }

        public static void LogExceptionAndExit(string message, Exception e)
        {
            File.WriteAllText("errors.log", message + e.Message);
            Console.WriteLine("Encountered a fatal error (see errors.log).\n  " + message + e.Message);
            Environment.Exit(1);
        }
    }
}
