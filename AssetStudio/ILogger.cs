using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    [Flags]
    public enum LoggerEvent
    {
        None = 0,
        Verbose = 1,
        Debug = 2,
        Info = 4,
        Warning = 8,
        Error = 16,
        All = Verbose | Debug | Info | Warning | Error,
    }

    public interface ILogger
    {
        public bool Silent { get; set; }
        public LoggerEvent Flags { get; set; }
        void Log(LoggerEvent loggerEvent, string message);
    }

    public sealed class DummyLogger : ILogger
    {
        public bool Silent { get; set; }
        public LoggerEvent Flags { get; set; }
        public void Log(LoggerEvent loggerEvent, string message) { }
    }

    public sealed class ConsoleLogger : ILogger
    {
        public bool Silent { get; set; }
        public LoggerEvent Flags { get; set; }
        public void Log(LoggerEvent loggerEvent, string message)
        {
            if (!Flags.HasFlag(loggerEvent) || Silent)
                return;

            Console.WriteLine("[{0}] {1}", loggerEvent, message);
        }
    }

    public sealed class FileLogger : ILogger
    {
        private const string LogFileName = "log.txt";
        private const string PrevLogFileName = "log_prev.txt";
        private readonly object LockWriter = new object();
        private StreamWriter Writer;

        public bool Silent { get; set; }
        public LoggerEvent Flags { get; set; }
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
            Dispose();
        }
        public void Log(LoggerEvent loggerEvent, string message)
        {
            if (!Flags.HasFlag(loggerEvent) || Silent)
                return;

            lock (LockWriter)
            {
                Writer.WriteLine($"[{DateTime.Now}][{loggerEvent}] {message}");
            }
        }

        public void Dispose()
        {
            Writer?.Dispose();
        }
    }
}
