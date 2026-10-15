using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Portfolio.Pixel;

/// <summary>
/// The fixed palette every piece of OS chrome draws from.
/// </summary>
public static class Palette
{
    public static readonly Color TitleBlue = Color.Parse("#2456C8");
    public static readonly Color TitleBlueLight = Color.Parse("#5B8FF5");
    public static readonly Color BorderBlue = Color.Parse("#123A9C");
    public static readonly Color TaskbarBlue = Color.Parse("#2A5FD6");
    public static readonly Color StartGreen = Color.Parse("#3DA83D");
    public static readonly Color StartGreenDark = Color.Parse("#267A26");
    public static readonly Color Paper = Color.Parse("#F4F1E6");
    public static readonly Color PaperDark = Color.Parse("#D8D2BE");
    public static readonly Color Bevel = Color.Parse("#8A8674");
    public static readonly Color Ink = Color.Parse("#1E1E2A");
    public static readonly Color InkSoft = Color.Parse("#5A5A6A");
    public static readonly Color CloseRed = Color.Parse("#D8452E");
    public static readonly Color Amber = Color.Parse("#E8A21C");
    public static readonly Color TerminalBack = Color.Parse("#0A120A");
    public static readonly Color TerminalGreen = Color.Parse("#6BE36B");
    public static readonly Color White = Colors.White;

    private static readonly Dictionary<Color, IImmutableSolidColorBrush> s_brushes = new();

    public static IImmutableSolidColorBrush Brush(Color color)
    {
        if (!s_brushes.TryGetValue(color, out var brush))
            s_brushes[color] = brush = new ImmutableSolidColorBrush(color);
        return brush;
    }
}
