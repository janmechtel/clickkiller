using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace clickkiller.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is App app)
        {
            app.TriggerReport();
        }
    }
}
