using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace clickkiller.Views;

public partial class MainWindow : Window
{
    private Menu? _mainMenu;
    private Rectangle? _menuTrigger;

    public MainWindow()
    {
        InitializeComponent();
        this.Closing += MainWindow_Closing;
        this.KeyDown += MainWindow_KeyDown;
        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        _mainMenu = this.FindControl<Menu>("MainMenu");
        _menuTrigger = this.FindControl<Rectangle>("MenuTrigger");

        if (_mainMenu != null && _menuTrigger != null)
        {
            _menuTrigger.PointerEntered += MenuTrigger_PointerEnter;
            _mainMenu.PointerExited += MainMenu_PointerLeave;
        }
    }

    private void MenuTrigger_PointerEnter(object? sender, PointerEventArgs e)
    {
        if (_mainMenu != null)
        {
            _mainMenu.IsVisible = true;
        }
    }

    private void MainMenu_PointerLeave(object? sender, PointerEventArgs e)
    {
        // if (_mainMenu != null && !_mainMenu.IsPointerOver && _menuTrigger != null && !_menuTrigger.IsPointerOver)
        // {
        //     _mainMenu.IsVisible = false;
        // }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        this.Hide();
    }

    private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Hide();
        }
    }
}
