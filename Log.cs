using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryEnforcer
{
    public static class Log
    {
        public static LogLevel LogLevel { get; set; }
        public static string LogDirectoryPath { get; set; }
        public static int LogArchiveDays { get; set; }

        private static bool _isInitialized = false;
        private static object _lockObject = new object();

        static Log()
        {
            lock (_lockObject)
            {
                if (!_isInitialized)
                {
                    LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), ConfigurationManager.AppSettings["LogLevel"]);
                    LogDirectoryPath = ConfigurationManager.AppSettings["LogDirectoryPath"];
                    LogArchiveDays = int.Parse(ConfigurationManager.AppSettings["LogArchiveDays"]);

                    Directory.CreateDirectory(LogDirectoryPath);
                    _isInitialized = true;
                }
            }
        }

        public static void Initialize()
        {
            // this just implicitly calls the static initializer above
        }

        public static bool IsTraceEnabled { get { return LogLevel >= LogLevel.Trace; } }
        public static bool IsDebugEnabled { get { return LogLevel >= LogLevel.Debug; } }
        public static bool IsInfoEnabled { get { return LogLevel >= LogLevel.Info; } }
        public static bool IsWarnEnabled { get { return LogLevel >= LogLevel.Warn; } }
        public static bool IsErrorEnabled { get { return LogLevel >= LogLevel.Error; } }
        public static bool IsFatalEnabled { get { return LogLevel >= LogLevel.Fatal; } }

        public static void Trace(string format, params object[] args)
        {
            Write(LogLevel.Trace, format, args);
        }

        public static void Debug(string format, params object[] args)
        {
            Write(LogLevel.Debug, format, args);
        }

        public static void Info(string format, params object[] args)
        {
            Write(LogLevel.Info, format, args);
        }

        public static void Warn(string format, params object[] args)
        {
            Write(LogLevel.Warn, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            Write(LogLevel.Error, format, args);
        }

        public static void Fatal(string format, params object[] args)
        {
            Write(LogLevel.Fatal, format, args);
        }

        public static void Write(LogLevel logLevel, string format, params object[] args)
        {
            if (LogLevel >= logLevel)
            {
                DateTime now = DateTime.Now;
                string logFileName = string.Format("RegistryEnforcer_{0:yyyy-MM-dd}.log", now);
                string logFilePath = Path.Combine(LogDirectoryPath, logFileName);
                lock (_lockObject)
                {
                    File.AppendAllText(logFilePath, string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}] {1} {2}{3}", now, logLevel, string.Format(format, args), Environment.NewLine));
                }
            }
        }
    }
}
