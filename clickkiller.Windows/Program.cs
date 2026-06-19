using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using Velopack;
using Velopack.Windows;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace clickkiller.Windows;

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

#pragma warning disable CA1416 // Validate platform compatibility
            // It's important to Run() the VelopackApp as early as possible in app startup.
            VelopackApp.Build()
                    .WithAfterInstallFastCallback((v) => new Shortcuts().CreateShortcutForThisExe(ShortcutLocation.Startup))
                    .WithFirstRun((v) => { /* Your first run code here */ })
                    .Run(logger);
#pragma warning restore CA1416 // Validate platform compatibility

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            Avalonia.Logging.Logger.Sink = new AvaloniaLoggingAdapter(logger);
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
            .UsePlatformDetect()
            .LogToILogger(logger)
            .WithInterFont()
            .UseReactiveUI()
            .AfterPlatformServicesSetup(builder =>
            {
                ClickKillerContainer.Initialize(AddServices());
            });

    public static ServiceCollection AddServices()
    {
        var collection = new ServiceCollection();

        //register all the things you want to inject here

        collection.AddSingleton<ILogger>(logger);

        return collection;
    }

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
