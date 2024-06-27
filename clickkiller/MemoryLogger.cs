using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace clickkiller;

public class LogUpdatedEventArgs : EventArgs
{
    public required string Text { get; set; }
}

public class MemoryLogger : ILogger
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public event EventHandler<LogUpdatedEventArgs> LogUpdated;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private readonly StringBuilder _sb = new StringBuilder();

#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
    public IDisposable? BeginScope<TState>(TState state)
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        lock (_sb) {
            var message = formatter(state, exception);
            if (exception != null) message += "\n" + exception.ToString();
            Console.WriteLine("log: " + message);
            _sb.AppendLine(message);
            LogUpdated?.Invoke(this, new LogUpdatedEventArgs { Text = _sb.ToString() });
        }
    }

    public override string ToString()
    {
        lock (_sb) {
            return _sb.ToString();
        }
    }
}
