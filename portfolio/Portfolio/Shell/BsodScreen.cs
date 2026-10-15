using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Portfolio.Pixel;

namespace Portfolio.Shell;

/// <summary>
/// The joke crash screen. Any key or click reboots.
/// </summary>
public class BsodScreen : Control
{
    private static readonly IBrush s_blue = Palette.Brush(Color.Parse("#0A1FA8"));
    private static readonly IBrush s_white = Palette.Brush(Palette.White);

    private static readonly string[] s_lines =
    {
        "A PROBLEM HAS BEEN DETECTED AND SUAREZ XP",
        "HAS STOPPED TO PROTECT YOUR PORTFOLIO.",
        "",
        "LAYOUT_CYCLE_DETECTED",
        "",
        "IF THIS IS THE FIRST TIME YOU SEE THIS SCREEN:",
        "",
        "* DO NOT CALL MEASURE FROM ARRANGE",
        "* CHECK IF IT WORKS ON YOUR MACHINE",
        "* BLAME THE DESIGNER",
        "* TELL THE USER IT IS A FEATURE",
        "",
        "TECHNICAL INFORMATION:",
        "*** STOP: 0X000000UI (0XC0FFEE, 0XDEADBEEF)",
        "",
        "PRESS ANY KEY TO REBOOT_",
    };

    public BsodScreen()
    {
        Focusable = true;
    }

    public event EventHandler? Dismissed;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = true;
        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        e.Handled = true;
        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(s_blue, new Rect(Bounds.Size));

        // Shrink the text on narrow screens so the longest line still fits.
        var longest = 0;
        foreach (var line in s_lines)
            longest = Math.Max(longest, line.Length);
        var px = Bounds.Width >= longest * PixelFont.Advance * 2 + 60 ? 2 : 1;

        for (var i = 0; i < s_lines.Length; i++)
            PixelFont.Draw(context, s_lines[i], 30, 36 + i * (PixelFont.GlyphHeight + 4) * px, px, s_white);
    }
}
