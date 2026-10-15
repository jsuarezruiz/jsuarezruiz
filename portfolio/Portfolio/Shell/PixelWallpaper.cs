using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Portfolio.Pixel;

namespace Portfolio.Shell;

/// <summary>
/// A pixel-art hill under a banded sky.
/// </summary>
public class PixelWallpaper : Control
{
    private const double Px = 4;

    private sealed record Mood(string[] Sky, string Cloud, string CloudShade, string FarHill, string Grass, string GrassLight);

    private static readonly Mood[] s_moods =
    {
        new(new[] { "#2F6FE0", "#4A8BEF", "#6FA8F7", "#9CC6FB" }, "#FFFFFF", "#D6E6FB", "#5FB85A", "#3F9E3C", "#57B84E"),
        new(new[] { "#4B3B8F", "#B0528A", "#F08A5D", "#F9C784" }, "#FFE3C2", "#F2B48A", "#6E8F4E", "#4C7A3A", "#6A9A4A"),
        new(new[] { "#0B1030", "#141B4A", "#1F2A66", "#2E3B80" }, "#8FA0D8", "#5A6AA8", "#1F5A3A", "#17452C", "#236B40"),
    };

    private int _mood;

    /// <summary>
    /// Day, dusk, night, and round again.
    /// </summary>
    public void NextMood()
    {
        _mood = (_mood + 1) % s_moods.Length;
        InvalidateVisual();
    }

    private static IBrush B(string hex) => Palette.Brush(Color.Parse(hex));

    public override void Render(DrawingContext context)
    {
        var cols = (int)Math.Ceiling(Bounds.Width / Px);
        var rows = (int)Math.Ceiling(Bounds.Height / Px);
        if (cols <= 0 || rows <= 0)
            return;

        var mood = s_moods[_mood];
        var band = Math.Max(1, rows * 0.62 / mood.Sky.Length);
        for (var i = 0; i < mood.Sky.Length; i++)
        {
            var top = Math.Floor(i * band) * Px;
            var bottom = i == mood.Sky.Length - 1 ? Bounds.Height : Math.Floor((i + 1) * band) * Px;
            context.FillRectangle(B(mood.Sky[i]), new Rect(0, top, Bounds.Width, bottom - top));
        }

        var cloud = B(mood.Cloud);
        var cloudShade = B(mood.CloudShade);
        Cloud(context, cloud, cloudShade, (int)(cols * 0.68), (int)(rows * 0.16), 22);
        Cloud(context, cloud, cloudShade, (int)(cols * 0.22), (int)(rows * 0.09), 14);
        Cloud(context, cloud, cloudShade, (int)(cols * 0.86), (int)(rows * 0.34), 10);

        var farHill = B(mood.FarHill);
        var grass = B(mood.Grass);
        var grassLight = B(mood.GrassLight);

        for (var x = 0; x < cols; x++)
        {
            var t = (double)x / cols;

            var far = (int)(rows * (0.56 - 0.05 * Math.Sin(t * Math.PI * 1.4 + 2.2)));
            context.FillRectangle(farHill, new Rect(x * Px, far * Px, Px, Bounds.Height - far * Px));

            var hill = HillTop(t, rows);
            context.FillRectangle(grassLight, new Rect(x * Px, hill * Px, Px, Px * 2));
            context.FillRectangle(grass, new Rect(x * Px, (hill + 2) * Px, Px, Bounds.Height - (hill + 2) * Px));
        }
    }

    private static int HillTop(double t, int rows)
        => (int)(rows * (0.66 - 0.09 * Math.Sin(t * Math.PI * 0.9 + 0.5)));

    private static void Cloud(DrawingContext context, IBrush cloud, IBrush shade, int x, int y, int w)
    {
        context.FillRectangle(shade, new Rect(x * Px, (y + 3) * Px, w * Px, 2 * Px));
        context.FillRectangle(cloud, new Rect((x + 1) * Px, (y + 1) * Px, (w - 2) * Px, 3 * Px));
        context.FillRectangle(cloud, new Rect((x + 3) * Px, y * Px, (w / 2) * Px, 2 * Px));
        context.FillRectangle(cloud, new Rect((x + w / 2) * Px, (y - 1) * Px, (w / 3) * Px, 2 * Px));
    }
}
