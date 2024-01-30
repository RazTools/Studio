using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public static class Logger
    {
        private static bool _fileLogging;

        public static ILogger Default = new DummyLogger();
        public static ILogger File;

        public static bool Silent { get; set; }
        public static LoggerEvent Flags { get; set; }

        public static bool FileLogging
        {
            get => _fileLogging;
            set
            {
                _fileLogging = value;
                if (_fileLogging)
                {
                    try
                    {
                        File = new FileLogger();
                    }
                    catch
                    {
                        _fileLogging = false;
                        Error("log file is already in use, disabling...");
                        return;
                    }
                }
                else
                {
                    ((FileLogger)File)?.Dispose();
                    File = null;
                }
            }
        }

        public static void Verbose(string message)
        {
            if (!Flags.HasFlag(LoggerEvent.Verbose) || Silent)
                return;

            try
            {
                var callerMethod = new StackTrace().GetFrame(1).GetMethod();
                var callerMethodClass = callerMethod.ReflectedType.Name;
                if (!string.IsNullOrEmpty(callerMethodClass))
                {
                    message = $"[{callerMethodClass}] {message}";
                }
            }
            catch (Exception) { }
            if (FileLogging) File.Log(LoggerEvent.Verbose, message);
            Default.Log(LoggerEvent.Verbose, message);
        }
        public static void Debug(string message)
        {
            if (!Flags.HasFlag(LoggerEvent.Debug) || Silent)
                return;

            if (FileLogging) File.Log(LoggerEvent.Debug, message);
            Default.Log(LoggerEvent.Debug, message);
        }
        public static void Info(string message)
        {
            if (!Flags.HasFlag(LoggerEvent.Info) || Silent)
                return;

            if (FileLogging) File.Log(LoggerEvent.Info, message);
            Default.Log(LoggerEvent.Info, message);
        }
        public static void Warning(string message)
        {
            if (!Flags.HasFlag(LoggerEvent.Warning) || Silent)
                return;

            if (FileLogging) File.Log(LoggerEvent.Warning, message);
            Default.Log(LoggerEvent.Warning, message);
        }
        public static void Error(string message)
        {
            if (!Flags.HasFlag(LoggerEvent.Error) || Silent)
                return;

            if (FileLogging) File.Log(LoggerEvent.Error, message);
            Default.Log(LoggerEvent.Error, message);
        }

        public static void Error(string message, Exception e)
        {
            if (!Flags.HasFlag(LoggerEvent.Error) || Silent)
                return;

            var sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine(e.ToString());

            message = sb.ToString();
            if (FileLogging) File.Log(LoggerEvent.Error, message);
            Default.Log(LoggerEvent.Error, message);
        }
    }
}
