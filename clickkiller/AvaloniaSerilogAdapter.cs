using Avalonia;
using Avalonia.Logging;
using Microsoft.Extensions.Logging;
using System;

namespace clickkiller
{


    // public static class MyLogExtensions
    // {
    //     public static AppBuilder LogToMySink(this AppBuilder builder, ILogSink?  logger,
    //         LogEventLevel level = LogEventLevel.Warning,
    //         params string[] areas)
    //     {
    //         Logger.Sink = logger;
    //         return builder;
    //     }
    // }
    
    public class AvaloniaLoggingAdapter : ILogSink
    {
        private readonly ILogger _logger;

        public AvaloniaLoggingAdapter(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsEnabled(LogEventLevel level, string area)
        {
            // Map Avalonia LogEventLevel to Microsoft.Extensions.Logging LogLevel
            return _logger.IsEnabled(MapLevel(level));
        }

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
        {
            // Use the logger to log the message
            _logger.Log(MapLevel(level), messageTemplate, source);
        }

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
        {
            throw new NotImplementedException();
        }

        private LogLevel MapLevel(LogEventLevel level)
        {
            return level switch
            {
                LogEventLevel.Verbose => LogLevel.Trace,
                LogEventLevel.Debug => LogLevel.Debug,
                LogEventLevel.Information => LogLevel.Information,
                LogEventLevel.Warning => LogLevel.Warning,
                LogEventLevel.Error => LogLevel.Error,
                LogEventLevel.Fatal => LogLevel.Critical,
                _ => LogLevel.None,
            };
        }
    }
}
