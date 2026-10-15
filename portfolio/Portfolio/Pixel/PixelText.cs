using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Portfolio.Pixel;

/// <summary>
/// Draws a single line of text with <see cref="PixelFont"/>.
/// </summary>
public class PixelText : Control
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<PixelText, string?>(nameof(Text));

    public static readonly StyledProperty<double> PixelSizeProperty =
        AvaloniaProperty.Register<PixelText, double>(nameof(PixelSize), 2);

    public static readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<PixelText, IBrush>(nameof(Foreground), Brushes.Black);

    public static readonly StyledProperty<IBrush?> ShadowProperty =
        AvaloniaProperty.Register<PixelText, IBrush?>(nameof(Shadow));

    static PixelText()
    {
        AffectsMeasure<PixelText>(TextProperty, PixelSizeProperty);
        AffectsRender<PixelText>(TextProperty, PixelSizeProperty, ForegroundProperty, ShadowProperty);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double PixelSize
    {
        get => GetValue(PixelSizeProperty);
        set => SetValue(PixelSizeProperty, value);
    }

    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public IBrush? Shadow
    {
        get => GetValue(ShadowProperty);
        set => SetValue(ShadowProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = PixelFont.Measure(Text, PixelSize);
        return Shadow is null ? size : new Size(size.Width + PixelSize, size.Height + PixelSize);
    }

    public override void Render(DrawingContext context)
    {
        if (Shadow is { } shadow)
            PixelFont.Draw(context, Text, PixelSize, PixelSize, PixelSize, shadow);
        PixelFont.Draw(context, Text, 0, 0, PixelSize, Foreground);
    }
}
