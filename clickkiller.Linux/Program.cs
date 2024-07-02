using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Velopack;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;

namespace clickkiller;

sealed class Program
{
    public static ILogger<Program> logger { get; private set; } = CreateLogger();

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
                .Run(logger);
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            string message = "Unhandled exception: " + ex.ToString();
            logger.LogError(message);
            Console.WriteLine(message);
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            // .LogToMySink(new AvaloniaLoggingAdapter(logger))
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .AfterSetup(builder =>
            {
                if (builder.Instance is not null) {
                    var app = (App)builder.Instance;
                    app.Logger = logger;
                }
            });

    static ILogger<Program> CreateLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(new LoggerConfiguration()
                .WriteTo.File(Path.Combine(App.appDataPath, "clickkiller.txt"), rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.Zero)
                .WriteTo.Console()
                .CreateLogger(), dispose: true);
        });
        return loggerFactory.CreateLogger<Program>();
    }
}
