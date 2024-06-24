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

