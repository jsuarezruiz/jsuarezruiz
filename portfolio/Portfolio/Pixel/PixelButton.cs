using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Portfolio.Pixel;

/// <summary>
/// A flat, hard-edged button with a bitmap-font label.
/// </summary>
public class PixelButton : Border
{
    private readonly PixelText _label;
    private Color _face;
    private bool _pressed;

    public PixelButton(string text, Color face, Color? foreground = null, double pixelSize = 2)
    {
        _face = face;
        _label = new PixelText
        {
            Text = text,
            PixelSize = pixelSize,
            Foreground = Palette.Brush(foreground ?? Palette.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        Child = _label;
        Padding = new Thickness(9, 6);
        BorderThickness = new Thickness(3);
        Cursor = new Cursor(StandardCursorType.Hand);
        UpdateVisual();
    }

    public event EventHandler? Click;

    public string? Text
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    public Color Face
    {
        get => _face;
        set { _face = value; UpdateVisual(); }
    }

    private void UpdateVisual()
    {
        Background = Palette.Brush(_pressed ? Shade(_face, 0.8) : _face);
        BorderBrush = Palette.Brush(Shade(_face, 0.6));
    }

    private static Color Shade(Color c, double f)
        => Color.FromRgb((byte)(c.R * f), (byte)(c.G * f), (byte)(c.B * f));

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _pressed = true;
        UpdateVisual();
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_pressed)
            return;
        _pressed = false;
        UpdateVisual();
        if (new Rect(Bounds.Size).Contains(e.GetPosition(this)))
        {
            Portfolio.Hosting.Host.PlaySound?.Invoke("click");
            Click?.Invoke(this, EventArgs.Empty);
        }
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _pressed = false;
        UpdateVisual();
    }
}
