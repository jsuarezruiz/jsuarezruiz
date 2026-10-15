using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace Portfolio.Pixel;

/// <summary>
/// A hand-drawn 5x7 bitmap font. Lowercase maps to uppercase.
/// </summary>
public static class PixelFont
{
    public const int GlyphWidth = 5;
    public const int GlyphHeight = 7;
    public const int Advance = GlyphWidth + 1;

    private static readonly Dictionary<char, string> s_glyphs = new()
    {
        ['A'] = "01110 10001 10001 11111 10001 10001 10001",
        ['B'] = "11110 10001 10001 11110 10001 10001 11110",
        ['C'] = "01110 10001 10000 10000 10000 10001 01110",
        ['D'] = "11110 10001 10001 10001 10001 10001 11110",
        ['E'] = "11111 10000 10000 11110 10000 10000 11111",
        ['F'] = "11111 10000 10000 11110 10000 10000 10000",
        ['G'] = "01110 10001 10000 10111 10001 10001 01111",
        ['H'] = "10001 10001 10001 11111 10001 10001 10001",
        ['I'] = "01110 00100 00100 00100 00100 00100 01110",
        ['J'] = "00111 00010 00010 00010 00010 10010 01100",
        ['K'] = "10001 10010 10100 11000 10100 10010 10001",
        ['L'] = "10000 10000 10000 10000 10000 10000 11111",
        ['M'] = "10001 11011 10101 10101 10001 10001 10001",
        ['N'] = "10001 11001 10101 10011 10001 10001 10001",
        ['O'] = "01110 10001 10001 10001 10001 10001 01110",
        ['P'] = "11110 10001 10001 11110 10000 10000 10000",
        ['Q'] = "01110 10001 10001 10001 10101 10010 01101",
        ['R'] = "11110 10001 10001 11110 10100 10010 10001",
        ['S'] = "01111 10000 10000 01110 00001 00001 11110",
        ['T'] = "11111 00100 00100 00100 00100 00100 00100",
        ['U'] = "10001 10001 10001 10001 10001 10001 01110",
        ['V'] = "10001 10001 10001 10001 10001 01010 00100",
        ['W'] = "10001 10001 10001 10101 10101 11011 10001",
        ['X'] = "10001 10001 01010 00100 01010 10001 10001",
        ['Y'] = "10001 10001 01010 00100 00100 00100 00100",
        ['Z'] = "11111 00001 00010 00100 01000 10000 11111",
        ['0'] = "01110 10001 10011 10101 11001 10001 01110",
        ['1'] = "00100 01100 00100 00100 00100 00100 01110",
        ['2'] = "01110 10001 00001 00010 00100 01000 11111",
        ['3'] = "11110 00001 00001 01110 00001 00001 11110",
        ['4'] = "00010 00110 01010 10010 11111 00010 00010",
        ['5'] = "11111 10000 11110 00001 00001 10001 01110",
        ['6'] = "00110 01000 10000 11110 10001 10001 01110",
        ['7'] = "11111 00001 00010 00100 01000 01000 01000",
        ['8'] = "01110 10001 10001 01110 10001 10001 01110",
        ['9'] = "01110 10001 10001 01111 00001 00010 01100",
        ['.'] = "00000 00000 00000 00000 00000 00000 00100",
        [','] = "00000 00000 00000 00000 00100 00100 01000",
        ['-'] = "00000 00000 00000 01110 00000 00000 00000",
        ['_'] = "00000 00000 00000 00000 00000 00000 11111",
        [':'] = "00000 00100 00000 00000 00000 00100 00000",
        ['/'] = "00001 00010 00010 00100 01000 01000 10000",
        ['!'] = "00100 00100 00100 00100 00100 00000 00100",
        ['?'] = "01110 10001 00001 00010 00100 00000 00100",
        ['>'] = "10000 01000 00100 00010 00100 01000 10000",
        ['<'] = "00001 00010 00100 01000 00100 00010 00001",
        ['*'] = "00000 00100 10101 01110 10101 00100 00000",
        ['+'] = "00000 00100 00100 11111 00100 00100 00000",
        ['='] = "00000 00000 11111 00000 11111 00000 00000",
        ['#'] = "01010 01010 11111 01010 11111 01010 01010",
        ['\''] = "00100 00100 00000 00000 00000 00000 00000",
        ['('] = "00010 00100 01000 01000 01000 00100 00010",
        [')'] = "01000 00100 00010 00010 00010 00100 01000",
        ['$'] = "00100 01111 10100 01110 00101 11110 00100",
        ['@'] = "01110 10001 10111 10101 10111 10000 01110",
    };

    private static readonly Dictionary<char, bool[]> s_bits = new();

    private static bool[]? Bits(char c)
    {
        c = char.ToUpperInvariant(c);

        // The font has no accents: fold the common ones onto their base letter.
        var folded = "ÁÀÄÂÉÈËÊÍÌÏÎÓÒÖÔÚÙÜÛÑÇ".IndexOf(c);
        if (folded >= 0)
            c = "AAAAEEEEIIIIOOOOUUUUNC"[folded];

        if (s_bits.TryGetValue(c, out var bits))
            return bits;
        if (!s_glyphs.TryGetValue(c, out var rows))
            return null;

        bits = new bool[GlyphWidth * GlyphHeight];
        var i = 0;
        foreach (var ch in rows)
        {
            if (ch == ' ')
                continue;
            bits[i++] = ch == '1';
        }
        s_bits[c] = bits;
        return bits;
    }

    public static Size Measure(string? text, double pixelSize)
    {
        if (string.IsNullOrEmpty(text))
            return new Size(0, GlyphHeight * pixelSize);
        return new Size((text.Length * Advance - 1) * pixelSize, GlyphHeight * pixelSize);
    }

    public static void Draw(DrawingContext context, string? text, double x, double y, double pixelSize, IBrush brush)
    {
        if (string.IsNullOrEmpty(text))
            return;

        for (var i = 0; i < text.Length; i++)
        {
            var bits = Bits(text[i]);
            if (bits is null)
                continue;

            var gx = x + i * Advance * pixelSize;
            for (var row = 0; row < GlyphHeight; row++)
            {
                // Merge horizontal runs so each glyph is a handful of rectangles.
                var col = 0;
                while (col < GlyphWidth)
                {
                    if (!bits[row * GlyphWidth + col])
                    {
                        col++;
                        continue;
                    }
                    var start = col;
                    while (col < GlyphWidth && bits[row * GlyphWidth + col])
                        col++;
                    context.FillRectangle(brush,
                        new Rect(gx + start * pixelSize, y + row * pixelSize, (col - start) * pixelSize, pixelSize));
                }
            }
        }
    }
}
