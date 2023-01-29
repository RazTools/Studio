using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public static class Logger
    {
        private static bool isDisposed = false;
        private static StreamWriter writer;

        public static ILogger Default = new DummyLogger();
        public static bool IsFileLogging = false;

        public static void Dispose()
        {
            if (!isDisposed)
            {
                writer.Dispose();
                isDisposed = true;
            }   
        }

        public static bool IsFileInit()
        {
            if (IsFileLogging && writer == null)
            {
                var path = Path.Combine(Environment.CurrentDirectory, "logs.txt");
                if (File.Exists(path))
                {
                    var prevPath = Path.Combine(Environment.CurrentDirectory, "logs-prev.txt");
                    File.Copy(path, prevPath, true);
                    File.Delete(path);
                }
                writer = File.CreateText(path);
                writer.AutoFlush = true;
            }
            return IsFileLogging;
        }

        public static void Verbose(string message)
        {
            var msg = Default.Log(LoggerEvent.Verbose, message);
            if (IsFileInit())
            {
                writer.WriteLine(msg);
            }
        }
        public static void Debug(string message)
        {
            var msg = Default.Log(LoggerEvent.Debug, message);
            if (IsFileInit())
            {
                writer.WriteLine(msg);
            }
        }
        public static void Info(string message)
        {
            var msg = Default.Log(LoggerEvent.Info, message);
            if (IsFileInit())
            {
                writer.WriteLine(msg);
            }
        }
        public static void Warning(string message)
        {
            var msg = Default.Log(LoggerEvent.Warning, message);
            if (IsFileInit())
            {
                writer.WriteLine(msg);
            }
        }
        public static void Error(string message)
        {
            var msg = Default.Log(LoggerEvent.Error, message);
            if (IsFileInit())
            {
                writer.WriteLine(msg);
            }
        }

        public static void Error(string message, Exception e)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine(e.ToString());
            var msg = Default.Log(LoggerEvent.Error, sb.ToString());
            if (IsFileLogging)
            {
                writer.WriteLine(msg);
                writer.Flush();
            }
        }
    }
}
