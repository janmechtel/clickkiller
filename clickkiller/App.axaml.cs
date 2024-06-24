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
using Avalonia.Threading;

namespace clickkiller;

public partial class App : Application
{
    private WindowIcon? _trayIcon;
    private MainWindow? _mainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnKeyReleased(object sender, KeyboardHookEventArgs e)
    {
        // Global Shortcut for F1
        if (e.Data.KeyCode == KeyCode.VcF1)
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

            var contextMenu = new NativeMenu();
            var exitMenuItem = new NativeMenuItem("Exit");
            exitMenuItem.Click += (sender, args) => 
            {
                Environment.Exit(0);
            };
            contextMenu.Items.Add(exitMenuItem);
            trayIcon.Menu = contextMenu;

            _mainWindow = new MainWindow();
            desktop.MainWindow = _mainWindow;  
            _mainWindow.Hide();

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

    public void TriggerReport()
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

        if (_mainWindow != null)
        {
          Dispatcher.UIThread.InvokeAsync(() =>
            {
                _mainWindow.Show();
                _mainWindow.Activate();
            });
        }
    }
}

