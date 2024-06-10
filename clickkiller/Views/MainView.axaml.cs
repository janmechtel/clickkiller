using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace clickkiller.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        App.TriggerReport();
    }
}
