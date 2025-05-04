using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using VirtualControlPanel.Models;
using VirtualControlPanel.ViewModels;

namespace VirtualControlPanel.Views;

public partial class PmdgCduWindow : Window
{
    private CduSettings? _cduSettings;

    public PmdgCduWindow()
    {
        InitializeComponent();

        Opened += (_, _) =>
        {
            if (DataContext is not PmdgCduViewModel pmdgCduViewModel)
            {
                return;
            }

            CduSettings cduSettings = pmdgCduViewModel.CduSettings;
            Width = cduSettings.Width;
            Height = cduSettings.Height;
            Position = new PixelPoint(cduSettings.PositionX, cduSettings.PositionY);
            WindowState = cduSettings.WindowState;
            _cduSettings = cduSettings; 
        };
    }

    private void InputElementOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || e.ClickCount != 2)
        {
            return;
        }

        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            return;
        }

        WindowState = WindowState.Maximized;
    }

    private void InputElementOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            return;
        }

        if (e is not { Key: Key.Enter, KeyModifiers: KeyModifiers.Alt })
        {
            return;
        }

        if (WindowState != WindowState.FullScreen)
        {
            WindowState = WindowState.FullScreen;
            return;
        }

        WindowState = WindowState.Normal;
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (_cduSettings is null)
        {
            return;
        }

        _cduSettings.PositionX = e.Point.X;
        _cduSettings.PositionY = e.Point.Y;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_cduSettings is null)
        {
            return;
        }

        _cduSettings.Width = e.NewSize.Width;
        _cduSettings.Height = e.NewSize.Height;
        _cduSettings.WindowState = WindowState;
    }
}