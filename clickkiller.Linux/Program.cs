using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using Velopack;
using Microsoft.Extensions.Logging;


namespace clickkiller;

sealed class Program
{
    public static MemoryLogger? Log { get; private set; }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try {
            // Logging is essential for debugging! Ideally you should write it to a file.
            Log = new MemoryLogger();

            // It's important to Run() the VelopackApp as early as possible in app startup.
            VelopackApp.Build()
                .WithFirstRun((v) => { /* Your first run code here */ })
                .Run(Log);


            Task.Run(UpdateApp).Wait();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        } catch (Exception ex) {
            string message = "Unhandled exception: " + ex.ToString();
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

    
    private static async Task UpdateApp()
    {
        Program.Log?.LogInformation("Updating app");

        var mgr = new UpdateManager("/home/janmechtel/Projects/ck/clickkiller/clickkiller.Linux/releases");

        // check for new version
        var newVersion = await mgr.CheckForUpdatesAsync();
        if (newVersion == null)
            return; // no update available

        // download new version
        await mgr.DownloadUpdatesAsync(newVersion);

        // install new version and restart app
        mgr.ApplyUpdatesAndRestart(newVersion);
    }
}
