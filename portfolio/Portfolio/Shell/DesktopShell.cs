using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Portfolio.Hosting;
using Portfolio.Pixel;

namespace Portfolio.Shell;

/// <summary>
/// The pixel OS: wallpaper, desktop icons, a window layer, a taskbar and a Start menu.
/// </summary>
public partial class DesktopShell : Grid
{
    private const double CellW = 186;
    private const double CellH = 93;

    private readonly Canvas _windows = new();
    private readonly StackPanel _tasks = new() { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
    private PixelWindow? _activeWindow;
    private readonly Canvas _icons = new() { Background = Brushes.Transparent };
    private readonly Canvas _menuLayer = new();
    private readonly PixelWallpaper _wallpaper = new();
    private Border? _contextMenu;
    private bool _iconsMoved;
    private readonly PixelText _clock = new() { Foreground = Palette.Brush(Palette.White), VerticalAlignment = VerticalAlignment.Center };
    private readonly Border _balloon;
    private readonly Border _startMenu;
    private readonly PixelButton _start = new("START", Palette.StartGreen) { Margin = new Thickness(0, 0, 9, 0) };
    private readonly DispatcherTimer _clockTimer = new() { Interval = TimeSpan.FromSeconds(20) };
    private readonly BootScreen _boot = new();
    private readonly BsodScreen _bsod = new();
    private readonly ScreensaverScreen _screensaver = new();
    private readonly DispatcherTimer _idleTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    private readonly HashSet<string> _opened = new();
    private readonly Border _toast;
    private IDisposable? _toastTimer;
    private DateTime _lastInput = DateTime.UtcNow;
    private bool _active;
    private int _z;
    private int _cascade;
    private bool _booted;

    public DesktopShell()
    {
        Current = this;
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        RowDefinitions = new RowDefinitions("*,Auto");

        Children.Add(_wallpaper);

        // Icons sit on a grid: drag one to move it, right-click for a menu.
        _icons.Children.Add(Icon(Art.Folder, "MY PROJECTS", "projects"));
        _icons.Children.Add(Icon(Art.Document, "RESUME.DOC", "resume"));
        _icons.Children.Add(Icon(Art.Globe, "INTERNET", "browser"));
        _icons.Children.Add(Icon(Art.Chat, "ASKGPT", "chat"));
        _icons.Children.Add(Icon(Art.Mail, "MAIL", "mail"));
        _icons.Children.Add(Icon(Art.Studio, "CODE STUDIO", "studio"));
        _icons.Children.Add(Icon(Art.Terminal, "TERMINAL", "terminal"));
        _icons.Children.Add(Icon(Art.Bug, "BUG HUNT", "game", 6));
        _icons.Children.Add(Icon(Art.Trophy, "ACHIEVEMENTS", "achievements"));
        _icons.Children.Add(Icon(Art.Bin, "RECYCLE BIN", "bin"));
        _icons.Children.Add(Icon(Art.Warning, "DONT_CLICK.EXE", "crash"));
        _icons.PointerPressed += OnDesktopPressed;
        _icons.SizeChanged += (_, _) =>
        {
            // Until the visitor moves an icon, keep them laid out for the current size.
            if (!_iconsMoved)
                ArrangeIcons();
        };
        Children.Add(_icons);

        _balloon = Balloon();
        Children.Add(_balloon);
        _windows.SizeChanged += (_, e) =>
        {
            foreach (var window in _windows.Children.OfType<PixelWindow>())
                window.Fill(e.NewSize);
        };
        Children.Add(_windows);

        _startMenu = StartMenu();
        Children.Add(_startMenu);

        _menuLayer.ZIndex = 900;
        Children.Add(_menuLayer);

        var taskbar = Taskbar();
        SetRow(taskbar, 1);
        Children.Add(taskbar);

        // The boot splash covers everything until it has played once.
        SetRowSpan(_boot, 2);
        _boot.ZIndex = 1000;
        _boot.Finished += (_, _) => Children.Remove(_boot);
        Children.Add(_boot);

        SetRowSpan(_bsod, 2);
        _bsod.ZIndex = 1001;
        _bsod.Dismissed += (_, _) => Reboot();

        SetRowSpan(_screensaver, 2);
        _screensaver.ZIndex = 1002;
        _screensaver.Dismissed += (_, _) => StopScreensaver();

        _toast = Toast();
        Children.Add(_toast);

        // Any press outside the menu closes it.
        AddHandler(PointerPressedEvent, OnAnyPointerPressed, RoutingStrategies.Tunnel);

        // Any input at all counts as activity, for the screensaver.
        AddHandler(PointerMovedEvent, (_, _) => _lastInput = DateTime.UtcNow, RoutingStrategies.Tunnel);
        AddHandler(KeyDownEvent, (_, _) => _lastInput = DateTime.UtcNow, RoutingStrategies.Tunnel);
        _idleTimer.Tick += (_, _) => CheckIdle();

        _clockTimer.Tick += (_, _) => UpdateClock();
        UpdateClock();
    }

    /// <summary>
    /// The live shell, so the host can open apps from outside (the 3D room).
    /// </summary>
    public static DesktopShell? Current { get; private set; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Achievements.Unlocked += ShowToast;
        _clockTimer.Start();
        _idleTimer.Start();

        // Inside the room the host calls Boot() when the visitor sits down.
        if (Host.StandUp is null)
            Boot();
    }

    /// <summary>
    /// The visitor sat down at the laptop. Plays the boot splash, the first time only.
    /// </summary>
    public void Boot()
    {
        _active = true;
        _lastInput = DateTime.UtcNow;
        if (_booted)
            return;
        _booted = true;
        _boot.Start();
    }

    /// <summary>
    /// The visitor stood up. Nothing should start, or keep animating, on a screen nobody sees.
    /// </summary>
    public void Leave()
    {
        _active = false;
        if (Children.Contains(_screensaver))
            StopScreensaver();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Achievements.Unlocked -= ShowToast;
        _clockTimer.Stop();
        _idleTimer.Stop();
        _active = false;
    }

    private void UpdateClock() => _clock.Text = DateTime.Now.ToString("HH:mm");

    private void OnAnyPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastInput = DateTime.UtcNow;
        if (e.Source is not Visual source)
            return;

        if (_contextMenu is { } menu && source != menu && !menu.IsVisualAncestorOf(source))
            CloseContextMenu();

        if (_startMenu.IsVisible && source != _start && !_startMenu.IsVisualAncestorOf(source) && !_start.IsVisualAncestorOf(source))
            _startMenu.IsVisible = false;
    }
}
