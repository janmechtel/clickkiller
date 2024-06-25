using Avalonia.Controls;

namespace clickkiller.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            ApplicationTextBox.AttachedToVisualTree += ApplicationTextBox_AttachedToVisualTree;
        }

        private void ApplicationTextBox_AttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
        {
            ApplicationTextBox.Focus();
        }
    }
}
