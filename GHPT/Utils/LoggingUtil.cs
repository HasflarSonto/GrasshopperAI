using System;
using System.IO;

namespace GHPT.Utils
{
    public static class LoggingUtil
    {
        private static readonly string LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "GHPT_Debug.log");

        public static void LogInfo(string message)
        {
            LogToFile($"[INFO] {message}");
        }

        public static void LogError(string message, Exception ex = null)
        {
            if (ex != null)
            {
                LogToFile($"[ERROR] {message}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
            else
            {
                LogToFile($"[ERROR] {message}");
            }
        }

        public static void LogDebug(string message)
        {
            LogToFile($"[DEBUG] {message}");
        }

        private static void LogToFile(string message)
        {
            try
            {
                File.AppendAllText(LogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // If we can't write to the log file, at least try to write to the console
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }
} 