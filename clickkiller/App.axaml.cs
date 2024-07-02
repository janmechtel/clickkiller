using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Logging;
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
using System.IO;

namespace clickkiller;

public partial class App : Application
{
    public ILogger Logger { get; }
    private WindowIcon? _trayIcon;
    
    private MainWindow? _mainWindow;
    private static FileStream? _lockFile;
    public static readonly string appDataPath = GetAppPath();

    public static void ExitApplication()
    {
        Environment.Exit(0);
    }

    public App(ILogger logger)
    {
        Logger = logger;
    }

    public override void Initialize()
    {
        if (IsNotRunning()) {
            Task.Run(UpdateApp).Wait();
            AvaloniaXamlLoader.Load(this);
        } else {
            Logger.LogInformation("Exiting now because the app is probably already running.");
            Environment.Exit(0);
        }
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
            exitMenuItem.Click += (sender, args) => ExitApplication();
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
                DataContext = new MainViewModel(appDataPath, ExitApplication, UpdateApp)
            };
            desktop.MainWindow = _mainWindow;
            _mainWindow.Hide();

        }
        else
        {
            throw new NotImplementedException();
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


    public async Task UpdateApp()
    {
        Logger.LogInformation("Updating app");
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
            Logger.LogError(ex.Message);
        }
    }

    private bool IsNotRunning()
    {

        if (OperatingSystem.IsLinux() || OperatingSystem.IsWindows())
        {
            string lockFilePath = Path.Combine(appDataPath, ".lock");
            try
            {
                // check platform to be linux or windows
                _lockFile = File.Open(lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _lockFile.Lock(0, 0);
                return true;
            }
            catch
            {
                Logger.LogError("Could not lock file {lockFilePath}.", lockFilePath);
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private static string GetAppPath()
    {
        if (IsDebugMode())
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
        else
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = "clickkiller";
            string fullPath = Path.Combine(appDataPath, appFolder);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }
    }
    private static bool IsDebugMode()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

}

