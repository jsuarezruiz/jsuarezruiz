using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Portfolio.Pixel;

namespace Portfolio.Shell;

/// <summary>
/// The screensaver: the OS logo drifting around a black screen, changing colour each time it
/// hits an edge. It keeps count of the corners it lands in exactly. Any input ends it.
/// </summary>
public class ScreensaverScreen : Control
{
    private const double Px = 4;
    private const string Logo = "SUAREZ XP";

    private static readonly IBrush s_black = Palette.Brush(Colors.Black);
    private static readonly IBrush s_dim = Palette.Brush(Color.Parse("#3A3A4A"));
    private static readonly IBrush[] s_colors =
    {
        Palette.Brush(Color.Parse("#5B8FF5")), Palette.Brush(Color.Parse("#6BE36B")), Palette.Brush(Color.Parse("#F2C230")),
        Palette.Brush(Color.Parse("#E5484D")), Palette.Brush(Color.Parse("#B07CF6")), Palette.Brush(Color.Parse("#4FD8E8")),
    };

    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(33) };
    private readonly Size _logo = PixelFont.Measure(Logo, Px);
    private double _x = 40, _y = 60, _dx = 2.1, _dy = 1.6;
    private int _color;
    private int _corners;
    private DateTime _started;
    private Point? _pointer;

    public ScreensaverScreen()
    {
        Focusable = true;
        _timer.Tick += (_, _) => Step();
    }

    public event EventHandler? Dismissed;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _started = DateTime.UtcNow;
        _pointer = null;
        _timer.Start();
        Focus();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    private void Step()
    {
        var maxX = Math.Max(0, Bounds.Width - _logo.Width);
        var maxY = Math.Max(0, Bounds.Height - _logo.Height);

        _x += _dx;
        _y += _dy;

        var hitX = _x <= 0 || _x >= maxX;
        var hitY = _y <= 0 || _y >= maxY;
        if (hitX)
        {
            _dx = -_dx;
            _x = Math.Clamp(_x, 0, maxX);
        }
        if (hitY)
        {
            _dy = -_dy;
            _y = Math.Clamp(_y, 0, maxY);
        }
        if (hitX || hitY)
            _color = (_color + 1) % s_colors.Length;
        if (hitX && hitY)
            _corners++;

        InvalidateVisual();
    }

    private void Dismiss() => Dismissed?.Invoke(this, EventArgs.Empty);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = true;
        Dismiss();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        // The pointer may settle for a moment after the screensaver appears; only a real move ends it.
        var at = e.GetPosition(this);
        _pointer ??= at;
        var moved = Math.Abs(at.X - _pointer.Value.X) + Math.Abs(at.Y - _pointer.Value.Y);
        if (moved > 12 && DateTime.UtcNow - _started > TimeSpan.FromMilliseconds(600))
            Dismiss();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        e.Handled = true;

        // The key that started it (Enter, in the Terminal) can still be arriving.
        if (DateTime.UtcNow - _started > TimeSpan.FromMilliseconds(400))
            Dismiss();
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(s_black, new Rect(Bounds.Size));
        PixelFont.Draw(context, Logo, Math.Floor(_x), Math.Floor(_y), Px, s_colors[_color]);
        PixelFont.Draw(context, $"CORNER HITS: {_corners}", 12, Bounds.Height - 26, 2, s_dim);
    }
}
