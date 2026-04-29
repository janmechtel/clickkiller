using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Velopack;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
            ApplyLinuxScaling();
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

    // On GNOME Wayland with fractional scaling (scale-monitor-framebuffer), the
    // compositor tells clients scale=1 and upscales the framebuffer itself, so
    // Avalonia sees scale=1 and renders the UI tiny on HiDPI displays.
    //
    // AVALONIA_GLOBAL_SCALE_FACTOR is the correct env var for Wayland (a plain
    // multiplier like "2"). AVALONIA_SCREEN_SCALE_FACTORS is X11-only (connector names).
    //
    // Detection order, matching what the Scarab/NixOS fix uses:
    //   1. gsettings org.gnome.desktop.interface scaling-factor  (0 = unset, skip)
    //   2. Xft.dpi from xrdb  (divide by 96 baseline)
    //
    // No-op if AVALONIA_GLOBAL_SCALE_FACTOR is already set externally.
    static void ApplyLinuxScaling()
    {
        if (!OperatingSystem.IsLinux()) return;
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AVALONIA_GLOBAL_SCALE_FACTOR"))) return;

        double scale = GetGsettingsScale() ?? GetXrdbScale() ?? GetNiriScale() ?? 1.0;

        if (scale <= 1.0) return;

        string value = scale.ToString("G6", System.Globalization.CultureInfo.InvariantCulture);
        Environment.SetEnvironmentVariable("AVALONIA_GLOBAL_SCALE_FACTOR", value);
        logger.LogInformation("Set AVALONIA_GLOBAL_SCALE_FACTOR={Value} (from system DPI settings)", value);
    }

    static double? GetGsettingsScale()
    {
        try
        {
            using var proc = Process.Start(new ProcessStartInfo("gsettings",
                "get org.gnome.desktop.interface scaling-factor")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            if (proc is null) return null;
            string output = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit();
            // returns "uint32 2" or "uint32 0" (0 means unset)
            var m = Regex.Match(output, @"(\d+)$");
            if (!m.Success) return null;
            int v = int.Parse(m.Groups[1].Value);
            return v > 0 ? (double)v : null;
        }
        catch { return null; }
    }

    static double? GetXrdbScale()
    {
        try
        {
            using var proc = Process.Start(new ProcessStartInfo("xrdb", "-query")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            if (proc is null) return null;
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            var m = Regex.Match(output, @"^Xft\.dpi:\s*(\d+)", RegexOptions.Multiline);
            if (!m.Success) return null;
            int dpi = int.Parse(m.Groups[1].Value);
            return Math.Round(dpi / 96.0, 6);
        }
        catch { return null; }
    }

    static double? GetNiriScale()
    {
        try
        {
            using var proc = Process.Start(new ProcessStartInfo("niri", "msg outputs")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            if (proc is null) return null;
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            // Look for "Scale: 1.5" or similar in the output
            var matches = Regex.Matches(output, @"Scale:\s*([\d.]+)");
            double maxScale = 0;
            foreach (Match m in matches)
            {
                if (double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double s))
                {
                    if (s > maxScale) maxScale = s;
                }
            }
            return maxScale > 0 ? maxScale : null;
        }
        catch { return null; }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .LogToILogger(logger)
            .UsePlatformDetect()
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
