using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Portfolio.Pixel;

namespace Portfolio.Shell;

public partial class DesktopShell
{
    /// <summary>
    /// Lays the icons out in columns, top to bottom.
    /// </summary>
    private void ArrangeIcons()
    {
        var rows = Math.Max(1, (int)((_icons.Bounds.Height - 9) / CellH));
        for (var i = 0; i < _icons.Children.Count; i++)
        {
            Canvas.SetLeft(_icons.Children[i], i / rows * CellW);
            Canvas.SetTop(_icons.Children[i], 9 + i % rows * CellH);
        }
    }

    private void OnDesktopPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            return;

        e.Handled = true;
        ShowContextMenu(e.GetPosition(_menuLayer),
            ("ARRANGE ICONS", () => { _iconsMoved = false; ArrangeIcons(); }
        ),
            ("REFRESH", Refresh),
            ("NEXT WALLPAPER", () => { _wallpaper.NextMood(); Achievements.Unlock("decorator"); }
        ),
            ("SCREENSAVER", StartScreensaver),
            ("OPEN TERMINAL", () => OpenApp("terminal")));
    }

    /// <summary>
    /// Does what Refresh has always done: blinks the icons and changes nothing.
    /// </summary>
    private void Refresh()
    {
        Achievements.Unlock("refresher");
        _icons.Opacity = 0;
        DispatcherTimer.RunOnce(() => _icons.Opacity = 1, TimeSpan.FromMilliseconds(140));
    }

    private Control Icon(string[] sprite, string label, string id, double pixelSize = 4)
    {
        var panel = new StackPanel
        {
            Spacing = 6,
            Width = CellW,
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        panel.Children.Add(new PixelSprite { Rows = sprite, PixelSize = pixelSize, HorizontalAlignment = HorizontalAlignment.Center });
        panel.Children.Add(new PixelText
        {
            Text = label,
            Foreground = Palette.Brush(Palette.White),
            Shadow = Palette.Brush(Palette.Ink),
            HorizontalAlignment = HorizontalAlignment.Center,
        });

        Point origin = default;
        Point grab = default;
        var pressed = false;
        var dragging = false;

        panel.PointerPressed += (_, e) =>
        {
            var point = e.GetCurrentPoint(this).Properties;
            e.Handled = true;
            if (point.IsRightButtonPressed)
            {
                ShowContextMenu(e.GetPosition(_menuLayer),
                    ("OPEN", () => OpenApp(id)),
                    ("DELETE", () => Say("CANNOT DELETE " + label, "IT IS LOAD-BEARING")));
                return;
            }
            if (!point.IsLeftButtonPressed)
                return;

            pressed = true;
            dragging = false;
            origin = e.GetPosition(_icons);
            grab = e.GetPosition(panel);
            e.Pointer.Capture(panel);
        };

        panel.PointerMoved += (_, e) =>
        {
            if (!pressed)
                return;

            var at = e.GetPosition(_icons);
            // A small wobble while clicking is still a click.
            if (!dragging && Math.Abs(at.X - origin.X) + Math.Abs(at.Y - origin.Y) < 6)
                return;

            dragging = true;
            panel.ZIndex = 1;
            panel.Opacity = 0.75;
            Canvas.SetLeft(panel, at.X - grab.X);
            Canvas.SetTop(panel, at.Y - grab.Y);
        };

        panel.PointerReleased += (_, e) =>
        {
            if (!pressed)
                return;
            pressed = false;

            // Releasing the capture raises PointerCaptureLost, which clears the drag state,
            // so note whether this was a drag before letting go.
            var wasDragging = dragging;
            e.Pointer.Capture(null);

            if (!wasDragging)
            {
                OpenApp(id);
                return;
            }

            // Drop onto the nearest grid cell inside the desktop.
            dragging = false;
            _iconsMoved = true;
            Achievements.Unlock("designer");
            panel.ZIndex = 0;
            panel.Opacity = 1;
            var maxCol = Math.Max(0, (int)((_icons.Bounds.Width - CellW) / CellW));
            var maxRow = Math.Max(0, (int)((_icons.Bounds.Height - 9 - CellH) / CellH));
            var col = Math.Clamp((int)Math.Round(Canvas.GetLeft(panel) / CellW), 0, maxCol);
            var row = Math.Clamp((int)Math.Round((Canvas.GetTop(panel) - 9) / CellH), 0, maxRow);
            Canvas.SetLeft(panel, col * CellW);
            Canvas.SetTop(panel, 9 + row * CellH);
        };

        panel.PointerCaptureLost += (_, _) =>
        {
            pressed = dragging = false;
            panel.Opacity = 1;
        };

        return panel;
    }

    private void ShowContextMenu(Point at, params (string Label, Action Action)[] entries)
    {
        CloseContextMenu();
        _startMenu.IsVisible = false;

        var items = new StackPanel();
        foreach (var (label, action) in entries)
            items.Children.Add(MenuItem(label, () => { CloseContextMenu(); action(); }));

        var menu = new Border
        {
            Child = items,
            Width = 210,
            Background = Palette.Brush(Palette.Paper),
            BorderBrush = Palette.Brush(Palette.BorderBlue),
            BorderThickness = new Thickness(3),
        };

        // Keep it on screen when opened near the right or bottom edge.
        var height = entries.Length * 32 + 6;
        Canvas.SetLeft(menu, Math.Max(0, Math.Min(at.X, _menuLayer.Bounds.Width - menu.Width)));
        Canvas.SetTop(menu, Math.Max(0, Math.Min(at.Y, _menuLayer.Bounds.Height - height)));
        _menuLayer.Children.Add(menu);
        _contextMenu = menu;
    }

    private void CloseContextMenu()
    {
        if (_contextMenu is null)
            return;
        _menuLayer.Children.Remove(_contextMenu);
        _contextMenu = null;
    }
}
