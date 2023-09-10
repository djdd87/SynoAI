namespace SynoAI
{
    using Microsoft.Extensions.Logging;
    using System;

    public class TimestampLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new TimestampLogger();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources here
            }

            // Dispose unmanaged resources here

            // Call base.Dispose() if this class has a base class that implements IDisposable
        }

    }

    public class TimestampLogger : ILogger
    {
        private readonly ILogger _consoleLogger; // Add a console logger

        public TimestampLogger()
        {
            _consoleLogger = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            }).CreateLogger("Console");
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // Get the current timestamp
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // Include the timestamp in the log message
            var logMessage = $"{timestamp} [{logLevel}] {formatter(state, exception)}";

            // Write the log message to the console
            _consoleLogger.Log(logLevel, eventId, state, exception, (_, _) => logMessage);
        }
    }
}
