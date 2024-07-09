using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace clickkiller.Views
{
    public partial class MainView : UserControl
    {
        public ILogger Logger { get; set; }

        public MainView()
        {
            Logger = ClickKillerContainer.ServiceProvider.GetRequiredService<ILogger>();

            InitializeComponent();
            ApplicationTextBox.AttachedToVisualTree += ApplicationTextBox_AttachedToVisualTree;

            Logger.LogInformation("hello from the main view");
        }

        private void ApplicationTextBox_AttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
        {
            ApplicationTextBox.Focus();
        }
    }
}
