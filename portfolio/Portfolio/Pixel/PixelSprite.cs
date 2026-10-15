using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Portfolio.Pixel;

public class PixelSprite : Control
{
    private string[] _rows = Array.Empty<string>();
    private double _pixelSize = 4;

    public string[] Rows
    {
        get => _rows;
        set { _rows = value; InvalidateMeasure(); InvalidateVisual(); }
    }

    public double PixelSize
    {
        get => _pixelSize;
        set { _pixelSize = value; InvalidateMeasure(); InvalidateVisual(); }
    }

    protected override Size MeasureOverride(Size availableSize)
        => _rows.Length == 0 ? default : new Size(_rows[0].Length * _pixelSize, _rows.Length * _pixelSize);

    public override void Render(DrawingContext context) => Art.Draw(context, _rows, 0, 0, _pixelSize);
}
