using Microsoft.Extensions.Logging;

namespace TableConverter.Utilities.Logging;

public class FileLogger : ILogger
{
    private readonly string _logName;
    private readonly FileLoggerProvider _loggerPrv;

    public FileLogger(string logName, FileLoggerProvider loggerPrv)
    {
        _logName = logName;
        _loggerPrv = loggerPrv;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null!;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _loggerPrv.MinLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        string message = formatter(state, exception);

        if (_loggerPrv.Options.FilterLogEntry != null)
        {
            if (!_loggerPrv.Options.FilterLogEntry(new LogMessage(_logName, logLevel, eventId, message, exception)))
            {
                return;
            }
        }

        if (_loggerPrv.FormatLogEntry != null)
        {
            _loggerPrv.WriteEntry(_loggerPrv.FormatLogEntry(new LogMessage(_logName, logLevel, eventId, message, exception)));
        }
        else
        {
            _loggerPrv.WriteEntry(
                Format.StringLogEntryFormatter.Instance.LowAllocLogEntryFormat(
                    _logName,
                    _loggerPrv.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now,
                    logLevel,
                    eventId,
                    message,
                    exception));
        }
    }
}