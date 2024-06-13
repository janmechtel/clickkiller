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
        var url = "https://clickkiller.com/ideas/";
        var psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };
        Process.Start(psi);
    }

}