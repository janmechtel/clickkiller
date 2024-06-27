using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using clickkiller.ViewModels;
using clickkiller.Views;
using Avalonia.Controls;
using System;
using Avalonia.Platform;
using SharpHook;
using SharpHook.Native;
using Avalonia.Threading;
using System.Threading.Tasks;
using Velopack;
using Microsoft.Extensions.Logging;

namespace clickkiller;

public partial class App : Application
{
    public static MemoryLogger? Log { get; private set; } = new MemoryLogger();
    private WindowIcon? _trayIcon;
    private MainWindow? _mainWindow;

    public override void Initialize()
    {
        Task.Run(UpdateApp).Wait();
        AvaloniaXamlLoader.Load(this);
    }

    private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
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

            var updateMenuItem = new NativeMenuItem("Update");
            updateMenuItem.Click += (sender, args) =>
            {
                Task.Run(UpdateApp).Wait();
            };
            contextMenu.Items.Add(updateMenuItem);

            trayIcon.Menu = contextMenu;

            _mainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
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

    private async void RegisterHook()
    {
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

        if (_mainWindow != null)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
              {
                  if (_mainWindow.IsVisible)
                  {
                      _mainWindow.Hide();
                  }
                  _mainWindow.Show();
              });
        }
    }


    private static async Task UpdateApp()
    {
        Log?.LogInformation("Updating app");
        try
        {


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
        catch (Exception ex)
        {
            Log?.LogError(ex.Message);
        }
    }

}

