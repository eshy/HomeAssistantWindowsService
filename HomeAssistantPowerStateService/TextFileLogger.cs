using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace HomeAssistantPowerStateService
{
    public class TextFileLogger : ILogger
    {
        private string _logLocation;

        public TextFileLogger(string logLocation)
        {
            _logLocation = logLocation;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message;
            if (formatter != null)
            {
                message = formatter(state, exception);
            }
            else
            {
                //message = LogFormatter.Formatter(state, exception);
                message = "No Log Formatter";
            }
            WriteToFile(message);
        }

        private void WriteToFile(string text)
        {
            string path = Path.Combine(_logLocation + "HomeAssistantServiceLog.txt");
            using (var writer = new StreamWriter(path, true))
            {
                writer.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")} - {text}");
                //writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }
    }

}
