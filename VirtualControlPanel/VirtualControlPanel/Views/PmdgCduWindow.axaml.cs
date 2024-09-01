using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using VirtualControlPanel.ViewModels;

namespace VirtualControlPanel.Views;

public partial class PmdgCduWindow : Window
{
    public PmdgCduWindow()
    {
        InitializeComponent();
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

    private bool _startup;

    private void WindowBaseOnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (DataContext is not PmdgCduViewModel pmdgCduViewModel)
        {
            return;
        }

        if (!_startup)
        {
            Position = new PixelPoint(pmdgCduViewModel.CduSettings.PositionX, pmdgCduViewModel.CduSettings.PositionY);
            _startup = true;
            return;
        }

        if (e.Point is { X: -32000, Y: 32000 })
        {
            return;
        }

        pmdgCduViewModel.CduSettings.PositionX = e.Point.X;
        pmdgCduViewModel.CduSettings.PositionY = e.Point.Y;
    }
    
    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is not PmdgCduViewModel pmdgCduViewModel)
        {
            return;
        }

        if (e.PreviousSize is { Width: 0, Height: 0 })
        {
            Width = pmdgCduViewModel.CduSettings.Width;
            Height = pmdgCduViewModel.CduSettings.Height;
            return;
        }
        
        pmdgCduViewModel.CduSettings.Width = e.NewSize.Width;
        pmdgCduViewModel.CduSettings.Height = e.NewSize.Height;
    }
}