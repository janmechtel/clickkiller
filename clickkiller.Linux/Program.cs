using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using Velopack;
using Microsoft.Extensions.Logging;


namespace clickkiller;

sealed class Program
{
    public static MemoryLogger Log { get; private set; } = new MemoryLogger();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try {
            // It's important to Run() the VelopackApp as early as possible in app startup.
            VelopackApp.Build()
                .WithFirstRun((v) => { /* Your first run code here */ })
                .Run(Log);

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        } catch (Exception ex) {
            string message = "Unhandled exception: " + ex.ToString();
            Log.LogError(message);
            Console.WriteLine(message);
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
