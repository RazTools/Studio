using System;
using System.Windows.Forms;

namespace AssetStudio.GUI
{
    class GUILogger : ILogger
    {
        public bool ShowErrorMessage = true;
        private Action<string> action;

        public bool Silent { get; set; }
        public LoggerEvent Flags { get; set; }

        public GUILogger(Action<string> action)
        {
            this.action = action;
        }

        public void Log(LoggerEvent loggerEvent, string message)
        {
            if (!Flags.HasFlag(loggerEvent) || Silent)
                return;

            switch (loggerEvent)
            {
                case LoggerEvent.Error:
                    if (ShowErrorMessage)
                    {
                        MessageBox.Show(message);
                    }
                    break;
                default:
                    action(message);
                    break;
            }
        }
    }
}
