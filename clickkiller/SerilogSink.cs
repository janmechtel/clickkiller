//using Avalonia;
//using Avalonia.Logging;
//using Serilog;
//using System;
//using System.IO;

//namespace clickkiller
//{


//    public static class MyLogExtensions
//    {
//        public static AppBuilder LogToMySink(this AppBuilder builder,  ILogger? logger,
//            LogEventLevel level = LogEventLevel.Warning,
//            params string[] areas)
//        {
//            Logger.Sink = new MyLogSink(logger, level, areas);
//            return builder;
//        }
//    }

//    public class MyLogSink : ILogSink
//    {
//        private readonly Serilog.Events.LogEventLevel _minimumLevel;
//        private readonly ILogger _logger;

//        public MyLogSink(ILogger? logger, LogEventLevel minimumLevel, params string[] areas)
//        {
//            _minimumLevel = MapLogLevel(minimumLevel);
//            _logger = logger ?? new LoggerConfiguration()
//                .WriteTo.Console()
//                .CreateLogger();
//        }

//        public bool IsEnabled(LogEventLevel level, string area)
//        {
//            return MapLogLevel(level) >= _minimumLevel;
//        }

//        public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
//        {
//            _logger.Write(MapLogLevel(level), "[{Area}] {Message}", area, messageTemplate);
//        }

//        public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
//        {
//            _logger.Write(MapLogLevel(level), "[{Area}] " + messageTemplate, area, propertyValues);
//        }

//        public void LogInformation(string messageTemplate, params object?[] propertyValues) {
//            _logger.Information(messageTemplate, propertyValues);
//        }
//        public void LogError(string messageTemplate, params object?[] propertyValues) {
//            _logger.Error(messageTemplate, propertyValues);
//        }

//        private Serilog.Events.LogEventLevel MapLogLevel(LogEventLevel level)
//        {
//            return level switch
//            {
//                LogEventLevel.Debug => Serilog.Events.LogEventLevel.Debug,
//                LogEventLevel.Information => Serilog.Events.LogEventLevel.Information,
//                LogEventLevel.Warning => Serilog.Events.LogEventLevel.Warning,
//                LogEventLevel.Error => Serilog.Events.LogEventLevel.Error,
//                LogEventLevel.Fatal => Serilog.Events.LogEventLevel.Fatal,
//                _ => Serilog.Events.LogEventLevel.Information
//            };
//        }
//    }
//}