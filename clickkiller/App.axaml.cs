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
using Microsoft.Extensions.DependencyInjection;
using System.IO.Pipes;

namespace clickkiller;

public partial class App : Application
{
    public ILogger Logger { get; set; }
    private WindowIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private static FileStream? _lockFile;
    private const string PipeName = "ClickKillerPipe";
    private const string TriggerReportMessage = "TriggerReport";
    public static readonly string appDataPath = GetAppPath();
    private UpdateManager _updateManager;

    public App()
    {
        Logger = ClickKillerContainer.ServiceProvider.GetRequiredService<ILogger>();
        _updateManager = new UpdateManager("https://storage.googleapis.com/clickkiller/");
    }

    public static void ExitApplication()
    {
        Environment.Exit(0);
    }

    public override void Initialize()
    {
        if (IsNotRunning())
        {
            Logger.LogInformation("Starting app");
            Task.Run(UpdateApp).Wait();
            _ = Task.Run(StartPipeServer);
            AvaloniaXamlLoader.Load(this);
        }
        else
        {
            Logger.LogInformation("App is probably already running.");
            Logger.LogInformation("Sending message to first instance to trigger report.");
            Task.Run(SendMessageToFirstInstance).Wait();
            Logger.LogInformation("Exiting now.");
            Environment.Exit(0);
        }
    }

    private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        // Global Shortcut for Alt+F1
        if (e.Data.KeyCode == KeyCode.VcF1 && e.RawEvent.Mask.HasAlt())
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

            var currentVersion = _updateManager.CurrentVersion;
            var updateMenuItemLabel = $"Update (Current: {currentVersion?.ToString() ?? "not installed"})";
            var updateMenuItem = new NativeMenuItem(updateMenuItemLabel);
            updateMenuItem.Click += async (sender, args) =>
            {
                await UpdateApp();
            };
            contextMenu.Items.Add(updateMenuItem);

            trayIcon.Menu = contextMenu;

            _mainWindow = new MainWindow
            {
                DataContext = new MainViewModel(appDataPath, ExitApplication, UpdateApp, updateMenuItemLabel)
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
            // check for new version
            var newVersion = await _updateManager.CheckForUpdatesAsync();
            if (newVersion == null)
            {
                Logger.LogInformation("No update available");
                return; // no update available
            }

            // download new version
            await _updateManager.DownloadUpdatesAsync(newVersion);

            // install new version and restart app
            _updateManager.ApplyUpdatesAndRestart(newVersion);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during update process");
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

    private void StartPipeServer()
    {
        Logger.LogInformation("Starting pipe server to listen for messages form other instances");
        using var server = new NamedPipeServerStream(PipeName);
        server.WaitForConnection();
        var reader = new StreamReader(server);
        while (true)
        {
            var message = reader.ReadLine();
            if (message == null)
            {
                Logger.LogInformation("Client disconnected, waiting for a new client connect...");
                server.Disconnect();
                server.WaitForConnection();
                continue;
            }

            if (message == TriggerReportMessage)
            {
                Logger.LogInformation("Received message via pipe, triggering report");
                TriggerReport();
            }
        }
    }
    private async Task SendMessageToFirstInstance()
    {
        using (var client = new NamedPipeClientStream(PipeName))
        {
            await client.ConnectAsync();
            using (var writer = new StreamWriter(client))
            {
                await writer.WriteLineAsync(TriggerReportMessage);
            }
        }
    }

}

