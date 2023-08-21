using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public enum LoggerEvent
    {
        Verbose,
        Debug,
        Info,
        Warning,
        Error,
    }

    public interface ILogger
    {
        void Log(LoggerEvent loggerEvent, string message, bool silent = false);
    }

    public sealed class DummyLogger : ILogger
    {
        public void Log(LoggerEvent loggerEvent, string message, bool silent = false) { }
    }

    public sealed class ConsoleLogger : ILogger
    {
        public void Log(LoggerEvent loggerEvent, string message, bool silent = false)
        {
            if (silent)
                return;

            Console.WriteLine("[{0}] {1}", loggerEvent, message);
        }
    }

    public sealed class FileLogger : ILogger
    {
        private const string LogFileName = "log.txt";
        private const string PrevLogFileName = "log_prev.txt";
        private readonly object LockWriter = new object();

        public StreamWriter Writer;

        public FileLogger()
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileName);
            var prevLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PrevLogFileName);

            if (File.Exists(logPath))
            {
                File.Move(logPath, prevLogPath, true);
            }
            Writer = new StreamWriter(logPath, true) { AutoFlush = true };
        }
        ~FileLogger()
        {
            Writer?.Dispose();
        }
        public void Log(LoggerEvent loggerEvent, string message, bool silent = false)
        {
            if (silent)
                return;

            lock (LockWriter)
            {
                Writer.WriteLine($"[{DateTime.Now}][{loggerEvent}] {message}");
            }
        }
    }
}
