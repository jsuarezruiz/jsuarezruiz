using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Portfolio.Content;
using Portfolio.Pixel;

namespace Portfolio.Shell;

/// <summary>
/// The boot splash: a BIOS self-test that prints line by line, then the OS logo with a
/// progress bar. Any click or key skips it.
/// </summary>
public class BootScreen : Control
{
    private const int TickMs = 50;
    private const int TicksPerLine = 5;
    private const int LogoTicks = 44;

    private static readonly IBrush s_black = Palette.Brush(Colors.Black);
    private static readonly IBrush s_bios = Palette.Brush(Color.Parse("#C8C8C8"));
    private static readonly IBrush s_ok = Palette.Brush(Palette.TerminalGreen);
    private static readonly IBrush s_white = Palette.Brush(Palette.White);
    private static readonly IBrush s_accent = Palette.Brush(Palette.Amber);
    private static readonly IBrush s_bar = Palette.Brush(Palette.TitleBlueLight);
    private static readonly IBrush s_barFrame = Palette.Brush(Color.Parse("#5A5A6A"));

    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(TickMs) };
    private readonly string[] _lines;
    private int _tick;

    public BootScreen()
    {
        Focusable = true;

        var profile = PortfolioContent.Current.Profile;
        _lines = new[]
        {
            "SUAREZ BIOS V26.10",
            "",
            "CPU      .NET ................. OK",
            "GPU      SKIA ................. OK",
            $"MEMORY   {profile.Stars} STARS .......... OK",
            $"STORAGE  {profile.Repos} REPOSITORIES ..... OK",
            "DRIVERS  COFFEE DUCK BULLDOG .. OK",
            "KEYBOARD NOT FOUND. PRESS F1",
            "",
            "STARTING SUAREZ XP",
        };

        _timer.Tick += (_, _) =>
        {
            _tick++;
            if (_tick >= _lines.Length * TicksPerLine + LogoTicks)
                Finish();
            else
                InvalidateVisual();
        };
    }

    public event EventHandler? Finished;

    public bool IsRunning => _timer.IsEnabled;

    public void Start()
    {
        _tick = 0;
        _timer.Start();
        Portfolio.Hosting.Host.PlaySound?.Invoke("beep");
        Focus();
    }

    private void Finish()
    {
        if (!_timer.IsEnabled)
            return;
        _timer.Stop();
        Portfolio.Hosting.Host.PlaySound?.Invoke("chime");
        Finished?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Finish();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        Finish();
        e.Handled = true;
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(s_black, new Rect(Bounds.Size));

        var biosTicks = _lines.Length * TicksPerLine;
        if (_tick < biosTicks)
        {
            const double px = 2;
            var shown = _tick / TicksPerLine + 1;
            for (var i = 0; i < shown && i < _lines.Length; i++)
            {
                var line = _lines[i];
                var y = 30 + i * (PixelFont.GlyphHeight + 4) * px;
                PixelFont.Draw(context, line, 30, y, px, s_bios);
                if (line.EndsWith(" OK", StringComparison.Ordinal))
                    PixelFont.Draw(context, "OK", 30 + (line.Length - 2) * PixelFont.Advance * px, y, px, s_ok);
            }
            return;
        }

        // Logo and progress bar, centred.
        var cx = Math.Floor(Bounds.Width / 2);
        var cy = Math.Floor(Bounds.Height / 2);

        const double logoPx = 6;
        var name = PixelFont.Measure("SUAREZ", logoPx);
        var left = Math.Floor(cx - (name.Width + 18 + PixelFont.Measure("XP", 4).Width) / 2);
        PixelFont.Draw(context, "SUAREZ", left, cy - 60, logoPx, s_white);
        PixelFont.Draw(context, "XP", left + name.Width + 18, cy - 60, 4, s_accent);

        const double barW = 180, barH = 18;
        var bx = cx - barW / 2;
        var by = cy + 24;
        context.FillRectangle(s_barFrame, new Rect(bx - 3, by - 3, barW + 6, barH + 6));
        context.FillRectangle(s_black, new Rect(bx, by, barW, barH));

        // Three blocks that slide across, the way the old loaders did.
        var offset = (_tick - biosTicks) * 9 % (barW + 54) - 54;
        for (var i = 0; i < 3; i++)
        {
            var x0 = Math.Max(bx, bx + offset + i * 18);
            var x1 = Math.Min(bx + barW, bx + offset + i * 18 + 14);
            if (x1 > x0)
                context.FillRectangle(s_bar, new Rect(x0, by + 3, x1 - x0, barH - 6));
        }
    }
}
