using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Portfolio.Content;
using Portfolio.Hosting;
using Portfolio.Pixel;

namespace Portfolio.Shell;

public partial class DesktopShell
{
    /// <summary>
    /// One button per open window: click to bring it forward, or to minimise it if it already is.
    /// </summary>
    private void RefreshTasks()
    {
        _tasks.Children.Clear();
        foreach (var window in _windows.Children.OfType<PixelWindow>())
        {
            var face = window == _activeWindow ? Palette.TitleBlueLight : window.IsMinimised ? Palette.BorderBlue : Palette.TitleBlue;
            var label = window.Title.Length > 11 ? window.Title[..11] : window.Title;
            var task = new PixelButton(label, face) { Padding = new Thickness(9, 5) };
            task.Click += (_, _) => OnTaskClicked(window);
            _tasks.Children.Add(task);
        }
    }

    private void OnTaskClicked(PixelWindow window)
    {
        if (window.IsMinimised)
        {
            window.Restore();
            Activate(window);
        }
        else if (window == _activeWindow)
        {
            window.Minimise();
        }
        else
        {
            Activate(window);
        }
    }

    private Border StartMenu()
    {
        var content = PortfolioContent.Current;
        var items = new StackPanel();

        items.Children.Add(new Border
        {
            Background = Palette.Brush(Palette.TitleBlue),
            Padding = new Thickness(12, 12),
            Child = new PixelText
            {
                Text = content.Profile.Name,
                PixelSize = 3,
                Foreground = Palette.Brush(Palette.White),
                Shadow = Palette.Brush(Palette.BorderBlue),
            },
        });

        items.Children.Add(MenuItem("MY PROJECTS", () => OpenApp("projects")));
        items.Children.Add(MenuItem("RESUME.DOC", () => OpenApp("resume")));
        items.Children.Add(MenuItem("ASKGPT", () => OpenApp("chat")));
        items.Children.Add(MenuItem("INTERNET", () => OpenApp("browser")));
        items.Children.Add(MenuItem("CODE STUDIO", () => OpenApp("studio")));
        items.Children.Add(MenuItem("TERMINAL", () => OpenApp("terminal")));
        items.Children.Add(MenuItem("BUG HUNT", () => OpenApp("game")));
        items.Children.Add(MenuItem("ACHIEVEMENTS", () => OpenApp("achievements")));
        items.Children.Add(Separator());

        foreach (var link in content.Links)
        {
            var isEmail = link.Url.StartsWith("mailto:", StringComparison.Ordinal);
            items.Children.Add(MenuItem(link.Label, isEmail ? () => OpenApp("mail") : () => OpenLink(link.Url)));
        }

        if (Host.StandUp is { } standUp)
        {
            items.Children.Add(Separator());
            items.Children.Add(MenuItem("STAND UP", () => { _startMenu.IsVisible = false; _active = false; standUp(); }));
        }

        return new Border
        {
            Child = items,
            Width = 264,
            IsVisible = false,
            Background = Palette.Brush(Palette.Paper),
            BorderBrush = Palette.Brush(Palette.BorderBlue),
            BorderThickness = new Thickness(3),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
        };
    }

    private static Control Separator() => new Border
    {
        Height = 3,
        Margin = new Thickness(9, 3),
        Background = Palette.Brush(Palette.PaperDark),
    };

    private static Control MenuItem(string label, Action action)
    {
        var text = new PixelText { Text = label, Foreground = Palette.Brush(Palette.Ink) };
        var row = new Border
        {
            Child = text,
            Padding = new Thickness(12, 9),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        row.PointerEntered += (_, _) =>
        {
            row.Background = Palette.Brush(Palette.TitleBlue);
            text.Foreground = Palette.Brush(Palette.White);
        };
        row.PointerExited += (_, _) =>
        {
            row.Background = Brushes.Transparent;
            text.Foreground = Palette.Brush(Palette.Ink);
        };
        row.Tapped += (_, _) => action();
        return row;
    }

    private static PixelButton TrayButton(string[] sprite, string tip)
        => TrayButton(new PixelSprite { Rows = sprite, PixelSize = 2 }, tip);

    /// <summary>
    /// A small icon-only button for the tray, named by its tooltip.
    /// </summary>
    private static PixelButton TrayButton(PixelSprite icon, string tip)
    {
        icon.HorizontalAlignment = HorizontalAlignment.Center;
        icon.VerticalAlignment = VerticalAlignment.Center;
        var button = new PixelButton("", Palette.TitleBlue) { Child = icon, Padding = new Thickness(6, 4), VerticalAlignment = VerticalAlignment.Center };
        ToolTip.SetTip(button, tip);
        return button;
    }

    private Control Taskbar()
    {
        _start.Click += (_, _) => _startMenu.IsVisible = !_startMenu.IsVisible;

        var tray = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 0, 12, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        if (Host.StandUp is { } standUp)
        {
            var leave = TrayButton(Art.Exit, "Stand up");
            leave.Click += (_, _) => { _active = false; standUp(); };
            tray.Children.Add(leave);
        }
        if (Host.ToggleSound is { } toggleSound)
        {
            var icon = new PixelSprite { Rows = Host.SoundOn?.Invoke() == false ? Art.SoundOff : Art.SoundOn, PixelSize = 2 };
            var sound = TrayButton(icon, "Sound on or off");
            sound.Click += (_, _) => icon.Rows = toggleSound() ? Art.SoundOn : Art.SoundOff;
            tray.Children.Add(sound);
        }
        tray.Children.Add(_clock);

        var bar = new DockPanel { Height = 39 };
        DockPanel.SetDock(_start, Dock.Left);
        DockPanel.SetDock(tray, Dock.Right);
        bar.Children.Add(_start);
        bar.Children.Add(tray);
        bar.Children.Add(new Border { ClipToBounds = true, Child = _tasks });

        return new Border
        {
            Background = Palette.Brush(Palette.TaskbarBlue),
            BorderBrush = Palette.Brush(Palette.TitleBlueLight),
            BorderThickness = new Thickness(0, 3, 0, 0),
            Child = bar,
        };
    }
}
