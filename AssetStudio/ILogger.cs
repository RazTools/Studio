using System;
using System.Collections.Generic;
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
        string Log(LoggerEvent loggerEvent, string message);
    }

    public sealed class DummyLogger : ILogger
    {
        public string Log(LoggerEvent loggerEvent, string message) => "";
    }

    public sealed class ConsoleLogger : ILogger
    {
        public string Log(LoggerEvent loggerEvent, string message)
        {
            var output = $"[{loggerEvent}] {message}";
            Console.WriteLine(output);
            return output;
        }
    }
}
