using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Logging;
using System;
using Velopack;

namespace clickkiller.Windows;

sealed class Program
{
    public static MemoryLogger Log { get; private set; } = new MemoryLogger();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // It's important to Run() the VelopackApp as early as possible in app startup.
            VelopackApp.Build()
                .WithFirstRun((v) => { /* Your first run code here */ })
                .Run(Log);

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            Avalonia.Logging.Logger.Sink = new AvaloniaLoggingAdapter(Log);
        }
        catch (Exception ex)
        {
            string message = "Unhandled exception: " + ex.ToString();
            Log.LogError(message);
            Console.WriteLine(message);
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var app = new App(Log);
        return AppBuilder.Configure(() => app)
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI();
    }
}
