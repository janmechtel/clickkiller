using Avalonia;
using Avalonia.Logging;
using Microsoft.Extensions.Logging;

namespace clickkiller
{
    public static class MyLogExtensions
    {
        public static AppBuilder LogToILogger(this AppBuilder builder, ILogger logger)
        {
            Logger.Sink = new AvaloniaLoggingAdapter(logger);
            return builder;
        }
    }
}
