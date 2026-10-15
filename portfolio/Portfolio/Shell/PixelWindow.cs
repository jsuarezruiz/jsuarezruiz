using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Portfolio.Pixel;

namespace Portfolio.Shell;

/// <summary>
/// A draggable OS window: pixel chrome around a paper-coloured, inset content area
/// where real (crisp) content lives. It can be minimised to the taskbar and maximised
/// to fill the desktop.
/// </summary>
public class PixelWindow : Border
{
    private const double Grid = 3;

    private readonly Border _titleBar;
    private readonly Control _content;
    private readonly bool _stretches;
    private Point _dragOrigin;
    private bool _dragging;

    // Where the window was, and how big its content was, before it was maximised.
    private double _left, _top, _contentWidth, _contentHeight;

    public PixelWindow(string id, string title, Control content, bool stretches = true)
    {
        Id = id;
        Title = title;
        _content = content;
        _stretches = stretches;

        var close = Chrome(new PixelText { Text = "X", Foreground = Palette.Brush(Palette.White) }, Palette.CloseRed);
        close.Click += (_, _) => Closed?.Invoke(this, EventArgs.Empty);

        var maximise = Chrome(new Border
        {
            Width = 12,
            Height = 10,
            BorderBrush = Palette.Brush(Palette.White),
            BorderThickness = new Thickness(2, 4, 2, 2),
        }, Palette.TitleBlueLight);
        maximise.Click += (_, _) => ToggleMaximise();

        var minimise = Chrome(new Border
        {
            Width = 12,
            Height = 3,
            Margin = new Thickness(0, 7, 0, 0),
            Background = Palette.Brush(Palette.White),
        }, Palette.TitleBlueLight);
        minimise.Click += (_, _) => Minimise();

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 3,
            Margin = new Thickness(0, 0, 3, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { minimise, maximise, close },
        };

        var titleRow = new Avalonia.Controls.Grid();
        titleRow.Children.Add(new Border
        {
            Height = 3,
            VerticalAlignment = VerticalAlignment.Top,
            Background = Palette.Brush(Palette.TitleBlueLight),
        });
        titleRow.Children.Add(new PixelText
        {
            Text = title,
            Foreground = Palette.Brush(Palette.White),
            Shadow = Palette.Brush(Palette.BorderBlue),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(9, 0, 0, 0),
        });
        titleRow.Children.Add(buttons);

        _titleBar = new Border
        {
            Height = 30,
            Background = Palette.Brush(Palette.TitleBlue),
            Child = titleRow,
            Cursor = new Cursor(StandardCursorType.SizeAll),
        };
        _titleBar.PointerPressed += OnTitlePressed;
        _titleBar.PointerMoved += OnTitleMoved;
        _titleBar.PointerReleased += (_, e) => { _dragging = false; e.Pointer.Capture(null); };
        _titleBar.DoubleTapped += (_, _) => ToggleMaximise();

        var client = new Border
        {
            Margin = new Thickness(3),
            BorderThickness = new Thickness(3),
            BorderBrush = Palette.Brush(Palette.Bevel),
            // Content that keeps its own size (the game) sits centred on black when maximised.
            Background = Palette.Brush(stretches ? Palette.Paper : Colors.Black),
            Child = content,
        };

        var layout = new DockPanel();
        DockPanel.SetDock(_titleBar, Dock.Top);
        layout.Children.Add(_titleBar);
        layout.Children.Add(client);

        Child = layout;
        Background = Palette.Brush(Palette.TitleBlue);
        BorderBrush = Palette.Brush(Palette.BorderBlue);
        BorderThickness = new Thickness(3);
    }

    public string Id { get; }

    public string Title { get; }

    public bool IsMaximised { get; private set; }

    public bool IsMinimised => !IsVisible;

    public event EventHandler? Closed;

    public event EventHandler? Activated;

    /// <summary>
    /// The window was minimised, restored or maximised.
    /// </summary>
    public event EventHandler? StateChanged;

    public void Minimise()
    {
        IsVisible = false;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Restore()
    {
        IsVisible = true;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ToggleMaximise()
    {
        if (Parent is not Canvas canvas)
            return;

        if (IsMaximised)
        {
            IsMaximised = false;
            Width = Height = double.NaN;
            _content.Width = _contentWidth;
            _content.Height = _contentHeight;
            Canvas.SetLeft(this, _left);
            Canvas.SetTop(this, _top);
        }
        else
        {
            IsMaximised = true;
            _left = Canvas.GetLeft(this);
            _top = Canvas.GetTop(this);
            _contentWidth = _content.Width;
            _contentHeight = _content.Height;
            if (_stretches)
                _content.Width = _content.Height = double.NaN;
            Canvas.SetLeft(this, 0);
            Canvas.SetTop(this, 0);
            Fill(canvas.Bounds.Size);
        }

        _titleBar.Cursor = new Cursor(IsMaximised ? StandardCursorType.Arrow : StandardCursorType.SizeAll);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Keeps a maximised window filling the desktop when the desktop changes size.
    /// </summary>
    public void Fill(Size desktop)
    {
        if (!IsMaximised)
            return;
        Width = desktop.Width;
        Height = desktop.Height;
    }

    private static PixelButton Chrome(Control glyph, Color face)
    {
        glyph.HorizontalAlignment = HorizontalAlignment.Center;
        glyph.VerticalAlignment = VerticalAlignment.Center;
        return new PixelButton("", face)
        {
            Child = glyph,
            Width = 27,
            Height = 24,
            Padding = new Thickness(0),
        };
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Activated?.Invoke(this, EventArgs.Empty);
    }

    private void OnTitlePressed(object? sender, PointerPressedEventArgs e)
    {
        Activated?.Invoke(this, EventArgs.Empty);
        if (IsMaximised)
            return;
        _dragging = true;
        _dragOrigin = e.GetPosition(this);
        e.Pointer.Capture(_titleBar);
    }

    private void OnTitleMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragging || Parent is not Canvas canvas)
            return;

        var p = e.GetPosition(canvas);
        var x = Math.Clamp(p.X - _dragOrigin.X, -Bounds.Width + 60, Math.Max(0, canvas.Bounds.Width - 60));
        var y = Math.Clamp(p.Y - _dragOrigin.Y, 0, Math.Max(0, canvas.Bounds.Height - 30));

        // Snap to the pixel grid so the chrome never lands on a half pixel.
        Canvas.SetLeft(this, Math.Round(x / Grid) * Grid);
        Canvas.SetTop(this, Math.Round(y / Grid) * Grid);
    }
}
