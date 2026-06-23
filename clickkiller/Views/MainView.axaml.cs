using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using clickkiller.ViewModels;
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
            AddHandler(InputElement.KeyDownEvent, MainView_KeyDown, RoutingStrategies.Tunnel);

            Logger.LogInformation("hello from the main view");
        }

        private void ApplicationTextBox_AttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
        {
            ApplicationTextBox.Focus();
        }

        private void MainView_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete && e.Key != Key.Back)
                return;

            if (ApplicationTextBox.IsFocused || NotesTextBox.IsFocused)
                return;

            if (IssuesDataGrid.SelectedItem is not IssueViewModel issue || DataContext is not MainViewModel viewModel)
                return;

            viewModel.ToggleIssueDoneStatusCommand.Execute(issue);
            e.Handled = true;
        }
    }
}
