using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using VirtualControlPanel.Models;
using VirtualControlPanel.ViewModels;

namespace VirtualControlPanel.Views;

public partial class MainWindow : Window
{
    private Settings? _settings;

    public MainWindow()
    {
        InitializeComponent();

        Opened += (_, _) =>
        {
            if (DataContext is not MainViewModel mainViewModel)
            {
                return;
            }

            Settings settings = mainViewModel.HomeViewModel.Settings;

            if (settings.Width != 0)
            {
                Width = settings.Width;
            }

            if (settings.Height != 0)
            {
                Height = settings.Height;
            }

            if (settings.PositionX != 0 || settings.PositionY != 0)
            {
                Position = new PixelPoint(settings.PositionX, settings.PositionY);
            }

            WindowState = settings.WindowState;

            if (settings.IsAutoStart)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    Dispatcher.UIThread.Post(() => WindowState = WindowState.Minimized);
                });
            }

            _settings = settings;
        };
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (_settings is null)
        {
            return;
        }

        _settings.PositionX = e.Point.X;
        _settings.PositionY = e.Point.Y;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_settings is null)
        {
            return;
        }

        _settings.Width = e.NewSize.Width;
        _settings.Height = e.NewSize.Height;
        _settings.WindowState = WindowState;
    }
}