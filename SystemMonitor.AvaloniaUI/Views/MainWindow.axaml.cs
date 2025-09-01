using Avalonia.Controls;
using SystemMonitor.AvaloniaUI.ViewModels;

namespace SystemMonitor.AvaloniaUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}