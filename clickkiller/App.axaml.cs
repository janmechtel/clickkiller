using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using clickkiller.ViewModels;
using clickkiller.Views;
using Avalonia.Controls;
using System.Diagnostics;
using System;
using Avalonia.Platform;
using SharpHook;
using SharpHook.Native;
using System.Runtime.InteropServices;
using System.IO;

namespace clickkiller;

public partial class App : Application
{
    private WindowIcon? _trayIcon;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnKeyReleased(object sender, KeyboardHookEventArgs e)
    {
        // Global Shortcut for Ctrl+Shift+Q
        if (e.Data.RawCode == 24 && e.RawEvent.Mask.HasCtrl()&& e.RawEvent.Mask.HasShift())
        {
            TriggerReport();
        }
    }


    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _trayIcon = new WindowIcon(AssetLoader.Open(new Uri("avares://clickkiller/Assets/clickkiller.ico")));
            var trayIcon = new TrayIcon
            {
                Icon = _trayIcon,
                ToolTipText = "ClickKiller",
                IsVisible = true
            };
            trayIcon.Clicked += OnTrayIconClicked;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        RegisterHook();

        base.OnFrameworkInitializationCompleted();
    }

    private async void RegisterHook() {
        var hook = new TaskPoolGlobalHook();

        hook.KeyReleased += OnKeyReleased;           // EventHandler<KeyboardHookEventArgs>

        await hook.RunAsync();

    }

    private void OnTrayIconClicked(object? sender, EventArgs e)
    {
        TriggerReport();
    }

public static void TriggerReport()
{
    string? homeDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? Environment.GetEnvironmentVariable("USERPROFILE")
        : Environment.GetEnvironmentVariable("HOME");

    string filePath = Path.Combine(homeDirectory, "reports.txt");

    if (!File.Exists(filePath))
    {
        File.Create(filePath).Dispose();
    }

    string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    // Read the existing content of the file
    string fileContent = File.ReadAllText(filePath);

    // Prepend the formatted date and time to the existing content
    fileContent = formattedDateTime + Environment.NewLine + fileContent;

    // Write the updated content back to the file
    File.WriteAllText(filePath, fileContent);

    var psi2 = new ProcessStartInfo
    {
        FileName = "code",
        Arguments = filePath,
        UseShellExecute = true
    };
    Process.Start(psi2);
}

}

