using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace Portfolio.Pixel;

/// <summary>
/// Sprites as rows of palette characters; '.' is transparent.
/// </summary>
public static class Art
{
    public static readonly Dictionary<char, Color> Colors = new()
    {
        ['d'] = Color.Parse("#3A2A1A"),
        ['y'] = Color.Parse("#FFE9A0"),
        ['Y'] = Color.Parse("#F2C230"),
        ['w'] = Color.Parse("#FFFFFF"),
        ['b'] = Color.Parse("#6F8FD8"),
        ['k'] = Color.Parse("#10141C"),
        ['g'] = Color.Parse("#6BE36B"),
        ['G'] = Color.Parse("#2F8F3A"),
        ['p'] = Color.Parse("#8B5CF6"),
        ['P'] = Color.Parse("#5B34C0"),
        ['r'] = Color.Parse("#E5484D"),
        ['R'] = Color.Parse("#9E2A2F"),
        ['s'] = Color.Parse("#F2C9A0"),
        ['h'] = Color.Parse("#3B2A22"),
        ['c'] = Color.Parse("#4FD8E8"),
        ['C'] = Color.Parse("#1F8FA8"),
        ['n'] = Color.Parse("#2A3A6A"),
        ['o'] = Color.Parse("#B0B4C0"),
    };

    public static readonly string[] Folder =
    {
        "............",
        ".dddd.......",
        "dyyyyddddddd",
        "dyyyyyyyyyyd",
        "dYYYYYYYYYYd",
        "dYYYYYYYYYYd",
        "dYYYYYYYYYYd",
        "dYYYYYYYYYYd",
        "dYYYYYYYYYYd",
        "dddddddddddd",
        "............",
        "............",
    };

    public static readonly string[] Document =
    {
        "..ddddddd...",
        "..dwwwwwdd..",
        "..dwwwwwdwd.",
        "..dwwwwwdddd",
        "..dwwwwwwwwd",
        "..dwbbbbbbwd",
        "..dwwwwwwwwd",
        "..dwbbbbbbwd",
        "..dwwwwwwwwd",
        "..dwbbbbwwwd",
        "..dwwwwwwwwd",
        "..dddddddddd",
    };

    public static readonly string[] Terminal =
    {
        "dddddddddddd",
        "dkkkkkkkkkkd",
        "dkgkkkkkkkkd",
        "dkkgkkkkkkkd",
        "dkgkkgggkkkd",
        "dkkkkkkkkkkd",
        "dkkkkkkkkkkd",
        "dkkkkkkkkkkd",
        "dddddddddddd",
        "....dddd....",
        "..dddddddd..",
        "............",
    };

    public static readonly string[] Mail =
    {
        "............",
        "dddddddddddd",
        "dwwwwwwwwwwd",
        "ddwwwwwwwwdd",
        "dwddwwwwddwd",
        "dwwwddddwwwd",
        "dwwwwwwwwwwd",
        "dwwwwwwwwwwd",
        "dwwwwwwwwwwd",
        "dddddddddddd",
        "............",
        "............",
    };

    public static readonly string[] Warning =
    {
        ".....dd.....",
        "....dYYd....",
        "....dYYd....",
        "...dYkkYd...",
        "...dYkkYd...",
        "..dYYkkYYd..",
        "..dYYkkYYd..",
        ".dYYYYYYYYd.",
        ".dYYYkkYYYd.",
        "dYYYYYYYYYYd",
        "dddddddddddd",
        "............",
    };

    public static readonly string[] Photos =
    {
        "............",
        "dddddddddddd",
        "dwwwwwwwwwwd",
        "dwccccccYYwd",
        "dwccccccYYwd",
        "dwccccgcccwd",
        "dwcccgggccwd",
        "dwcgggggggwd",
        "dwggggggggwd",
        "dwwwwwwwwwwd",
        "dddddddddddd",
        "............",
    };

    public static readonly string[] Bin =
    {
        "....dddd....",
        ".dddddddddd.",
        ".doooooooood",
        "..dddddddd..",
        "..dowowowd..",
        "..dowowowd..",
        "..dowowowd..",
        "..dowowowd..",
        "..dowowowd..",
        "..dooooood..",
        "..dddddddd..",
        "............",
    };

    public static readonly string[] Globe =
    {
        "....dddd....",
        "..ddccccdd..",
        ".dccGGcccCd.",
        ".dcGGGGccCd.",
        "dccGGGcccCCd",
        "dcccGccGGCCd",
        "dcccccGGGCCd",
        "dccccccGGCCd",
        ".dccccccGCd.",
        ".dCCccccCCd.",
        "..ddCCCCdd..",
        "....dddd....",
    };

    public static readonly string[] Chat =
    {
        "............",
        ".dddddddddd.",
        "dwwwwwwwwwwd",
        "dwwwwwwwwwwd",
        "dwwpwwpwwpwd",
        "dwwwwwwwwwwd",
        "dwwwwwwwwwwd",
        ".dddwwddddd.",
        "...dwwd.....",
        "...dwd......",
        "...dd.......",
        "............",
    };

    public static readonly string[] Trophy =
    {
        "..dddddddd..",
        "ddYYYYYYYYdd",
        "dYdYYyYYYdYd",
        "dYdYYyYYYdYd",
        ".ddYYyYYYdd.",
        "...dYYYYd...",
        "....dYYd....",
        ".....dd.....",
        ".....dd.....",
        "...dddddd...",
        "..dYYYYYYd..",
        "..dddddddd..",
    };

    public static readonly string[] Locked =
    {
        "............",
        "....oooo....",
        "...o....o...",
        "...o....o...",
        "..oooooooo..",
        "..oooooooo..",
        "..ooo..ooo..",
        "..ooo..ooo..",
        "..oooooooo..",
        "..oooooooo..",
        "............",
        "............",
    };

    public static readonly string[] Studio =
    {
        "............",
        ".........PP.",
        "..p.....pPP.",
        ".ppp...ppPP.",
        "pppp..pppPP.",
        ".pppppp.pPP.",
        "..pppp..pPP.",
        ".pppppp.pPP.",
        "pppp..pppPP.",
        ".ppp...ppPP.",
        "..p.....pPP.",
        ".........PP.",
    };

    public static readonly string[] SoundOn =
    {
        "....w.....",
        "...ww..w..",
        "wwwww...w.",
        "wwwww.w.w.",
        "wwwww.w.w.",
        "wwwww...w.",
        "...ww..w..",
        "....w.....",
    };

    public static readonly string[] SoundOff =
    {
        "....w.....",
        "...ww.....",
        "wwwww.w.w.",
        "wwwww..w..",
        "wwwww..w..",
        "wwwww.w.w.",
        "...ww.....",
        "....w.....",
    };

    public static readonly string[] Exit =
    {
        "wwwwww....",
        "w....w.w..",
        "w....w..w.",
        "w..wwwwwww",
        "w..wwwwwww",
        "w....w..w.",
        "w....w.w..",
        "wwwwww....",
    };

    public static readonly string[] BugFlyUp =
    {
        "w......w",
        "ww....ww",
        ".wkRRkw.",
        ".RrrrrR.",
        "RrwrrwrR",
        "RrrrrrrR",
        ".RrrrrR.",
        "........",
    };

    public static readonly string[] BugFlyDown =
    {
        "........",
        "........",
        "..kRRk..",
        "wRrrrrRw",
        "wrwrrwrw",
        "RrrrrrrR",
        ".RrrrrR.",
        "........",
    };

    public static readonly string[] Coffee =
    {
        "..o..o..",
        ".o..o...",
        "........",
        "wwwwww..",
        "wddddwww",
        "wddddw.w",
        "wddddwww",
        ".wwww...",
    };

    public static readonly string[] Duck =
    {
        "........",
        "...YY...",
        "..YkYrr.",
        "..YYY...",
        "YYYYYY..",
        "YYYYYYY.",
        ".YYYYY..",
        "........",
    };

    public static readonly string[] Player =
    {
        "..hhhh..",
        ".hhhhhh.",
        ".hsssss.",
        ".sskssk.",
        ".ssssss.",
        "..pppp..",
        ".pppppp.",
        ".pPppPp.",
        "..nnnn..",
        "..n..n..",
    };

    public static readonly string[] PlayerStep =
    {
        "..hhhh..",
        ".hhhhhh.",
        ".hsssss.",
        ".sskssk.",
        ".ssssss.",
        "..pppp..",
        ".pppppp.",
        ".pPppPp.",
        "..nnnn..",
        ".n....n.",
    };

    public static readonly string[] Gem =
    {
        "..cccc..",
        ".cwcccC.",
        "cwcccccC",
        "ccccccCC",
        ".ccccCC.",
        "..ccCC..",
        "...CC...",
        "........",
    };

    public static readonly string[] Bug =
    {
        "........",
        ".k....k.",
        "..kRRk..",
        ".RrrrrR.",
        "RrwrrwrR",
        "RrrrrrrR",
        ".RrrrrR.",
        "k.k..k.k",
    };

    public static void Draw(DrawingContext context, string[] rows, double x, double y, double pixelSize, bool flip = false)
    {
        for (var row = 0; row < rows.Length; row++)
        {
            var line = rows[row];
            var col = 0;
            while (col < line.Length)
            {
                var ch = line[flip ? line.Length - 1 - col : col];
                if (ch == '.' || !Colors.TryGetValue(ch, out var color))
                {
                    col++;
                    continue;
                }
                var start = col;
                while (col < line.Length && line[flip ? line.Length - 1 - col : col] == ch)
                    col++;
                context.FillRectangle(Palette.Brush(color),
                    new Rect(x + start * pixelSize, y + row * pixelSize, (col - start) * pixelSize, pixelSize));
            }
        }
    }
}
