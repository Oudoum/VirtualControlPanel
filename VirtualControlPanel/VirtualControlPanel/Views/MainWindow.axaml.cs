using Avalonia.Controls;
using VirtualControlPanel.ViewModels;

namespace VirtualControlPanel.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is MainViewModel { HomeViewModel.Settings.IsAutoStart: true })
            {
                WindowState = WindowState.Minimized;
            }
        };
    }
}