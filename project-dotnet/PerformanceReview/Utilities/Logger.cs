using System;

namespace PerformanceReview.Utilities
{
    public static class Logger
    {
        public static void Info(string message)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [INFO] {message}");
        }

        public static void Error(string message)
        {
            Console.Error.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR] {message}");
        }

        public static void Warn(string message)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [WARN] {message}");
        }
    }
}
