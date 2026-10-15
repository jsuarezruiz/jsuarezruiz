using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Portfolio.Hosting;
using Portfolio.Pixel;

namespace Portfolio.Shell;

public partial class DesktopShell
{
    private static readonly string[] s_appIds = { "projects", "resume", "browser", "chat", "mail", "studio", "terminal", "game", "bin" };
    private static readonly TimeSpan s_idle = TimeSpan.FromSeconds(60);

    private void CheckIdle()
    {
        // Only while the visitor is at the laptop, and nothing else has taken over the screen.
        if (!_active || Children.Contains(_screensaver) || Children.Contains(_boot) || Children.Contains(_bsod))
            return;
        if (DateTime.UtcNow - _lastInput >= s_idle)
            StartScreensaver();
    }

    public void StartScreensaver()
    {
        CloseContextMenu();
        _startMenu.IsVisible = false;
        if (Children.Contains(_screensaver))
            return;
        Children.Add(_screensaver);
        Achievements.Unlock("afk");
    }

    private void StopScreensaver()
    {
        Children.Remove(_screensaver);
        _lastInput = DateTime.UtcNow;
    }

    private readonly PixelText _toastTitle = new() { Foreground = Palette.Brush(Palette.Ink) };

    private Border Toast()
    {
        var text = new StackPanel
        {
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new PixelText { Text = "ACHIEVEMENT UNLOCKED", Foreground = Palette.Brush(Palette.InkSoft) },
                _toastTitle,
            },
        };
        var icon = new PixelSprite { Rows = Art.Trophy, PixelSize = 3, Margin = new Thickness(0, 0, 12, 0), VerticalAlignment = VerticalAlignment.Center };
        DockPanel.SetDock(icon, Dock.Left);

        var toast = new Border
        {
            IsVisible = false,
            ZIndex = 950,
            Child = new DockPanel { Children = { icon, text } },
            Padding = new Thickness(12, 9),
            Margin = new Thickness(0, 12, 12, 0),
            Background = Palette.Brush(Color.Parse("#FFF8C8")),
            BorderBrush = Palette.Brush(Palette.Amber),
            BorderThickness = new Thickness(3),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        toast.Tapped += (_, _) => { toast.IsVisible = false; OpenApp("achievements"); };
        return toast;
    }

    private void ShowToast(Achievement achievement)
    {
        Host.PlaySound?.Invoke("achievement");
        _toastTitle.Text = achievement.Title;
        _toast.IsVisible = true;
        _toastTimer?.Dispose();
        _toastTimer = DispatcherTimer.RunOnce(() => _toast.IsVisible = false, TimeSpan.FromSeconds(4.5));
    }

    /// <summary>
    /// Shows a short message in the balloon by the clock.
    /// </summary>
    private void Say(params string[] lines)
    {
        var stack = new StackPanel { Spacing = 6 };
        for (var i = 0; i < lines.Length; i++)
            stack.Children.Add(new PixelText { Text = lines[i], Foreground = Palette.Brush(i == 0 ? Palette.Ink : Palette.InkSoft) });
        _balloon.Child = stack;
        _balloon.IsVisible = true;
    }

    /// <summary>
    /// The blue screen. Everything open is lost, as it should be.
    /// </summary>
    public void Crash()
    {
        Host.PlaySound?.Invoke("error");
        CloseContextMenu();
        _startMenu.IsVisible = false;
        if (!Children.Contains(_bsod))
            Children.Add(_bsod);
    }

    private void Reboot()
    {
        Achievements.Unlock("survivor");
        Children.Remove(_bsod);
        _windows.Children.Clear();
        _activeWindow = null;
        RefreshTasks();
        _cascade = 0;
        if (!Children.Contains(_boot))
            Children.Add(_boot);
        _boot.Start();
    }

    private Border Balloon()
    {
        var lines = new StackPanel { Spacing = 6 };
        lines.Children.Add(new PixelText { Text = "WELCOME TO SUAREZ XP", Foreground = Palette.Brush(Palette.Ink) });
        lines.Children.Add(new PixelText { Text = "CLICK AN ICON TO OPEN IT", Foreground = Palette.Brush(Palette.InkSoft) });
        lines.Children.Add(new PixelText { Text = "DRAG TO MOVE, RIGHT-CLICK FOR MORE", Foreground = Palette.Brush(Palette.InkSoft) });

        var balloon = new Border
        {
            Child = lines,
            Padding = new Thickness(12, 9),
            Margin = new Thickness(0, 0, 12, 12),
            Background = Palette.Brush(Color.Parse("#FFF8C8")),
            BorderBrush = Palette.Brush(Palette.Ink),
            BorderThickness = new Thickness(3),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        balloon.PointerPressed += (_, _) => balloon.IsVisible = false;
        return balloon;
    }
}
