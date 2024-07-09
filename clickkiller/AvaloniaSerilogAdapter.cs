using Avalonia;
using Avalonia.Logging;
using Avalonia.Utilities;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;

using System;
using System.Text;

namespace clickkiller
{
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
            if (!IsEnabled(level, area))
            {
                return;
            }

            // Log the formatted message
            _logger.Log(MapLevel(level), Format(area, messageTemplate, source, propertyValues), source);
        }

        private static string Format(
            string area,
            string template,
            object? source,
            object?[] v)
        {
            var result = new StringBuilder(template.Length);
            var r = new CharacterReader(template.AsSpan());
            var i = 0;

            result.Append('[');
            result.Append(area);
            result.Append(']');

            while (!r.End)
            {
                var c = r.Take();

                if (c != '{')
                {
                    result.Append(c);
                }
                else
                {
                    if (r.Peek != '{')
                    {
                        result.Append('\'');
                        result.Append(i < v.Length ? v[i++] : null);
                        result.Append('\'');
                        r.TakeUntil('}');
                        r.Take();
                    }
                    else
                    {
                        result.Append('{');
                        r.Take();
                    }
                }
            }

            FormatSource(source, result);
            return result.ToString();
        }

        private static void FormatSource(object? source, StringBuilder result)
        {
            if (source is null)
                return;

            result.Append(" (");
            result.Append(source.GetType().Name);
            result.Append(" #");

            if (source is StyledElement se && se.Name is not null)
                result.Append(se.Name);
            else
                result.Append(source.GetHashCode());

            result.Append(')');
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
