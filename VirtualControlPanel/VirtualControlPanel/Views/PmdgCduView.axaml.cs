using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using VirtualControlPanel.Models;
using VirtualControlPanel.ViewModels;

namespace VirtualControlPanel.Views;

public partial class PmdgCduView : UserControl
{
    private bool _editorMode;

    private const FontWeight DefaultFontWeight = (FontWeight)550;

    private readonly FontFamily _pmdgFontNormal = new("avares://VirtualControlPanel/Assets/Fonts#PMDG_NGXu_DU_B");
    private readonly FontFamily _pmdgFontsSmall = new("avares://VirtualControlPanel/Assets/Fonts#PMDG_NGXu_DU_C");

    private readonly SolidColorBrush _transparent = new(Colors.Transparent);
    private readonly SolidColorBrush _gray = new(Colors.Gray);

    private readonly SolidColorBrush _white = new(Colors.White);
    private readonly SolidColorBrush _cyan = new(Colors.Cyan);
    private readonly SolidColorBrush _green = new(Color.FromRgb(0x10, 0xEF, 0x22));
    private readonly SolidColorBrush _magenta = new(Colors.Magenta);
    private readonly SolidColorBrush _amber = new(Color.FromRgb(0xF4, 0xCD, 0x2A));
    private readonly SolidColorBrush _red = new(Colors.Red);

    private readonly Thickness _zero = new(0);
    private readonly Thickness _one = new(1);

    private CduSettings _cduSettings = new();
    
    public PmdgCduView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        KeyDownEvent.AddClassHandler<TopLevel>(InputElementOnKeyDown, handledEventsToo: true);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is PmdgCduViewModel pmdgCduViewModel)
        {
            _cduSettings = pmdgCduViewModel.CduSettings;
            pmdgCduViewModel.SignalRClientService.PmdgDataReceived += (location, data) =>
            {
                if (location == pmdgCduViewModel.Title)
                {
                    InvokeSetPmdgCduCells(data);
                }
            };
        }

        CreatePmdgCduCells();
        CduGrid.Width = _cduSettings.GridWidth;
        CduGrid.Height = _cduSettings.GridHeight;
        CduGrid.RenderTransform = new ScaleTransform(_cduSettings.ScaleX, _cduSettings.ScaleY);
    }
    
    private void CreatePmdgCduCells()
    {
        double characterSize = _cduSettings.CharacterSize;
        Thickness margin = new(_cduSettings.MarginLeft, _cduSettings.MarginTop, _cduSettings.MarginRight, _cduSettings.MarginBottom);
        for (int column = 0; column < 24; column++)
        {
            for (int row = 0; row < 14; row++)
            {
                Border border = new()
                {
                    Background = _transparent,
                    OpacityMask = _gray,
                    BorderBrush = _white,
                    BorderThickness = _zero,
                    Margin = margin,
                };
                TextBlock txtBlock = new()
                {
                    ClipToBounds = false,
                    FontSize = characterSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    FontWeight = FontWeight.Normal,
                    Padding = _zero,
                };
                border.Child = txtBlock;
                Grid.SetColumn(border, column);
                Grid.SetRow(border, row);
                CduGrid.Children.Add(border);
            }
        }
    }

    private int? _brightness;

    public int? Brightness
    {
        get => _brightness;
        set
        {
            if (_brightness == value || value is null)
            {
                return;
            }

            _brightness = value;
            CduGrid.Opacity = (double)(1d / 4095 * value);
        }
    }

    private void InvokeSetPmdgCduCells(byte[] cduScreenData)
    {
        Dispatcher.UIThread.Invoke(() => SetPmdgCduCells(cduScreenData));
    }

    private void SetPmdgCduCells(byte[] cduScreenData)
    {
        if (CduGrid.Children.Count <= 0)
        {
            return;
        }

        // byte brightness = 0;
        int cellFactor = 0;
        for (int i = 0; i < Cdu.CduCells; i++)
        {
            int cellNumber = i + cellFactor;
            Border border = (Border)CduGrid.Children[i];
            TextBlock txtBlock = (TextBlock)border.Child!;

            char symbol = (char)cduScreenData[cellNumber];
            txtBlock.Text = symbol.ToString();
            
            // if (symbol == 'ë' && Brightness is null)
            // {
            //     CduGrid.Opacity = 1d / 23 * ++brightness;
            // }
            // if (brightness == 0 && columnCount == 23 && rowCount == 13 && symbol == '-' && cell.Flags == Cdu.Flags.Unused && Brightness is null)
            // {
            //     CduGrid.Opacity = 0;
            // }

            if (_editorMode)
            {
                border.BorderThickness = _one;
            }

            byte color = cduScreenData[cellNumber + 1];
            txtBlock.Foreground = SetForeground(color);

            byte flags = cduScreenData[cellNumber + 2];
            SetTextBlockWithFlags(flags, txtBlock, border);

            // if (columnCount == 0 && rowCount == 0)
            // {
            //     txtBlock.FontFamily = _pmdgFontBig;
            // }

            cellFactor += 2;
        }
    }

    private SolidColorBrush? SetForeground(byte color)
    {
        return (Cdu.Color)color switch
        {
            Cdu.Color.White => _white,
            Cdu.Color.Cyan => _cyan,
            Cdu.Color.Green => _green,
            Cdu.Color.Magenta => _magenta,
            Cdu.Color.Amber => _amber,
            Cdu.Color.Red => _red,
            _ => null
        };
    }

    private void SetTextBlockWithFlags(byte flags, TextBlock txtBlock, Border border)
    {
        switch ((Cdu.Flags)flags)
        {
            case Cdu.Flags.Default:
                txtBlock.FontFamily = _pmdgFontNormal;
                txtBlock.FontWeight = FontWeight.Normal;
                txtBlock.Opacity = 1;
                border.Background = _transparent;
                break;

            case Cdu.Flags.SmallFont:
                txtBlock.FontFamily = _pmdgFontsSmall;
                txtBlock.FontWeight = DefaultFontWeight;
                txtBlock.Opacity = 1;
                border.Background = _transparent;
                break;

            case Cdu.Flags.Reverse:
                txtBlock.FontFamily = _pmdgFontNormal;
                txtBlock.FontWeight = FontWeight.Normal;
                txtBlock.Opacity = 1;
                border.Background = _gray;
                break;

            case Cdu.Flags.SmallFont | Cdu.Flags.Reverse:
                txtBlock.FontFamily = _pmdgFontsSmall;
                txtBlock.FontWeight = DefaultFontWeight;
                txtBlock.Opacity = 1;
                border.Background = _gray;
                break;

            case Cdu.Flags.Unused:
                txtBlock.FontFamily = _pmdgFontNormal;
                txtBlock.FontWeight = FontWeight.Normal;
                txtBlock.Opacity = 0.5;
                txtBlock.Foreground = _white;
                border.Background = _transparent;
                break;

            case Cdu.Flags.SmallFont | Cdu.Flags.Unused:
                txtBlock.FontFamily = _pmdgFontsSmall;
                txtBlock.FontWeight = DefaultFontWeight;
                txtBlock.Opacity = 0.5;
                border.Background = _transparent;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(flags), flags, null);
        }
    }

    public async Task ClearPmdgCduCellsAsync()
    {
        await Task.Delay(500);
        AllCduGridChildren(border =>
        {
            if (border.Child is TextBlock textBlock)
            {
                textBlock.Text = null;
            }
        });
    }

    private void InputElementOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            return;
        }

        _editorMode = !_editorMode;
        AllCduGridChildren(border => { border.BorderThickness = border.BorderThickness == _zero ? _one : _zero; });
    }
    
    private void InputElementOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!_editorMode)
        {
            return;
        }

        switch (e.KeyModifiers)
        {
            case KeyModifiers.Alt:
                case KeyModifiers.Control:
                    case KeyModifiers.Shift:
                break;

            case KeyModifiers.None:
            case KeyModifiers.Meta:
            default:
                return;
        }

        double scaleX = _cduSettings.ScaleX;
        double scaleY = _cduSettings.ScaleY;
        
        switch (e.Delta.Y)
        {
            case > 0 when e.KeyModifiers == KeyModifiers.Control && scaleX < 3 && scaleY < 3:
                scaleX += 0.01;
                scaleY += 0.01;
                break;

            case < 0 when e.KeyModifiers == KeyModifiers.Control && scaleX > 0.3 && scaleY > 0.3:
                scaleX -= 0.01;
                scaleY -= 0.01;
                break;
            
            case > 0 when e.KeyModifiers == KeyModifiers.Shift && scaleX < 3:
                scaleX += 0.01;
                break;

            case < 0 when e.KeyModifiers == KeyModifiers.Shift && scaleX > 0.3:
                scaleX -= 0.01;
                break;
            
            case > 0 when e.KeyModifiers == KeyModifiers.Alt && scaleY < 3:
                scaleY += 0.01;
                break;

            case < 0 when e.KeyModifiers == KeyModifiers.Alt && scaleY > 0.3:
                scaleY -= 0.01;
                break;
        }

        CduGrid.RenderTransform = new ScaleTransform(scaleX, scaleY);
        _cduSettings.ScaleX = scaleX;
        _cduSettings.ScaleY = scaleY;
    }

    private void InputElementOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_editorMode || !IsPointerOver)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.F when e.KeyModifiers == KeyModifiers.Shift:
                _cduSettings.CharacterSize++;
                SetFontSizedCduGridChildren();
                break;

            case Key.F when _cduSettings.CharacterSize > 1:
                _cduSettings.CharacterSize--;
                SetFontSizedCduGridChildren();
                break;

            case Key.Space when e.KeyModifiers == KeyModifiers.Shift:
                ResetHeightAndWidth();
                break;
            
            case Key.Enter when e.KeyModifiers == KeyModifiers.Alt:
                break;

            case Key.Enter when e.KeyModifiers == KeyModifiers.Shift:
                _cduSettings.ScaleX = 1;
                _cduSettings.ScaleY = 1;
                CduGrid.RenderTransform = new ScaleTransform(_cduSettings.ScaleX, _cduSettings.ScaleY);
                ResetHeightAndWidth();
                AllCduGridChildren(border =>
                    {
                        if (border.Child is TextBlock textBlock)
                        {
                            textBlock.FontSize = _cduSettings.CharacterSize = 50;
                        }

                        border.Margin = new Thickness(0);
                        _cduSettings.MarginLeft = 0;
                        _cduSettings.MarginTop = 0;
                        _cduSettings.MarginRight = 0;
                        _cduSettings.MarginBottom = 0;
                    }
                );
                break;

            case Key.PageUp when e.KeyModifiers == KeyModifiers.Shift:
                _cduSettings.MarginTop--;
                SetMarginCduGridChildren();
                break;

            case Key.PageUp:
                _cduSettings.MarginBottom++;
                SetMarginCduGridChildren();
                break;

            case Key.PageDown when e.KeyModifiers == KeyModifiers.Shift:
                _cduSettings.MarginTop++;
                SetMarginCduGridChildren();
                break;

            case Key.PageDown:
                _cduSettings.MarginBottom--;
                SetMarginCduGridChildren();
                break;

            case Key.Add when e.KeyModifiers == KeyModifiers.Shift:
                _cduSettings.MarginRight++;
                SetMarginCduGridChildren();
                break;

            case Key.Add:
                _cduSettings.MarginLeft--;
                SetMarginCduGridChildren();
                break;

            case Key.Subtract when e.KeyModifiers == KeyModifiers.Shift:
                _cduSettings.MarginRight--;
                SetMarginCduGridChildren();
                break;

            case Key.Subtract:
                _cduSettings.MarginLeft++;
                SetMarginCduGridChildren();
                break;

            case Key.Up:
                _cduSettings.MarginTop--;
                _cduSettings.MarginBottom++;
                SetMarginCduGridChildren();
                break;

            case Key.Left:
                _cduSettings.MarginLeft--;
                _cduSettings.MarginRight++;
                SetMarginCduGridChildren();
                break;

            case Key.Down:
                _cduSettings.MarginTop++;
                _cduSettings.MarginBottom--;
                SetMarginCduGridChildren();
                break;

            case Key.Right:
                _cduSettings.MarginLeft++;
                _cduSettings.MarginRight--;
                SetMarginCduGridChildren();
                break;

            case Key.W:
                CduGrid.Height = CduGrid.Bounds.Height;
                CduGrid.Height = _cduSettings.GridHeight = --CduGrid.Height;
                break;

            case Key.A:
                CduGrid.Width = CduGrid.Bounds.Width;
                CduGrid.Width = _cduSettings.GridWidth = ++CduGrid.Width;
                break;

            case Key.S:
                CduGrid.Height = CduGrid.Bounds.Height;
                CduGrid.Height = _cduSettings.GridHeight = ++CduGrid.Height;
                break;

            case Key.D:
                CduGrid.Width = CduGrid.Bounds.Width;
                CduGrid.Width = _cduSettings.GridWidth = --CduGrid.Width;
                break;
        }
    }

    private delegate void CellAction(Border border);

    private void SetFontSizedCduGridChildren()
    {
        AllCduGridChildren(border =>
        {
            if (border.Child is TextBlock textBlock)
            {
                textBlock.FontSize = _cduSettings.CharacterSize;
            }
        });
    }

    private void SetMarginCduGridChildren()
    {
        Thickness margin = new(_cduSettings.MarginLeft, _cduSettings.MarginTop, _cduSettings.MarginRight, _cduSettings.MarginBottom);
        AllCduGridChildren(border => border.Margin = margin);
    }

    private void AllCduGridChildren(CellAction action)
    {
        foreach (Border? border in CduGrid.Children.Cast<Border?>())
        {
            if (border is not null)
            {
                action(border);
            }
        }
    }

    private void ResetHeightAndWidth()
    {
        CduGrid.Width = CduGrid.Bounds.Width;
        CduGrid.Width = _cduSettings.GridWidth = 620;
        CduGrid.Height = CduGrid.Bounds.Height;
        CduGrid.Height = _cduSettings.GridHeight = 550;
    }
}