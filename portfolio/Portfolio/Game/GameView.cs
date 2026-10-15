using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Portfolio.Content;
using Portfolio.Hosting;
using Portfolio.Pixel;

namespace Portfolio.Game;

/// <summary>
/// Bug Hunt: a tile platformer drawn with plain rectangles. Catch every bug with the net
/// (or stomp it) before shipping. The gems are the top repositories of the Xamarin era.
/// </summary>
public partial class GameView : Control
{
    private const int ViewW = 240;
    private const int ViewH = 136;
    private const int Tile = 8;
    private const int MapW = 110;
    private const int MapH = 17;

    private const double Gravity = 0.25;
    private const double JumpSpeed = -4.6;
    private const double RunSpeed = 1.5;
    private const double PlayerW = 6;
    private const double PlayerH = 10;
    private const double Step = 1000.0 / 60.0;
    private const int SwingFrames = 16;
    private const int MaxLives = 3;
    private const int CheatLives = 30;

    /// <summary>
    /// Three lives, or thirty for whoever typed the Konami code in the room.
    /// </summary>
    private static int StartLives => Host.ReadSetting?.Invoke("cheat") == "lives" ? CheatLives : MaxLives;

    private static readonly string[] s_heart = { ".rr.rr.", "rrrrrrr", "rrrrrrr", ".rrrrr.", "..rrr..", "...r..." };
    private static readonly string[] s_heartLost = { ".oo.oo.", "ooooooo", "ooooooo", ".ooooo.", "..ooo..", "...o..." };

    private static readonly string[] s_bugNames =
    {
        "NULLREFERENCEEXCEPTION", "LAYOUT CYCLE", "OFF BY ONE", "WORKS ON MY MACHINE",
        "RACE CONDITION", "MEMORY LEAK", "IT'S A FEATURE",
    };

    private static readonly IBrush s_sky = Palette.Brush(Color.Parse("#8FB8F8"));
    private static readonly IBrush s_hills = Palette.Brush(Color.Parse("#7FA6E8"));
    private static readonly IBrush s_dirt = Palette.Brush(Color.Parse("#7A4E2C"));
    private static readonly IBrush s_grass = Palette.Brush(Color.Parse("#3F9E3C"));
    private static readonly IBrush s_title = Palette.Brush(Palette.TitleBlue);
    private static readonly IBrush s_titleLight = Palette.Brush(Palette.TitleBlueLight);
    private static readonly IBrush s_close = Palette.Brush(Palette.CloseRed);
    private static readonly IBrush s_pole = Palette.Brush(Color.Parse("#E8E8F0"));
    private static readonly IBrush s_flag = Palette.Brush(Palette.Amber);
    private static readonly IBrush s_ink = Palette.Brush(Palette.Ink);
    private static readonly IBrush s_white = Palette.Brush(Palette.White);
    private static readonly IBrush s_shade = Palette.Brush(Color.FromArgb(200, 16, 20, 40));
    private static readonly IBrush s_netPole = Palette.Brush(Color.Parse("#7A4E2C"));
    private static readonly IBrush s_netMesh = Palette.Brush(Color.FromArgb(190, 255, 255, 255));

    private enum State { Title, Playing, Won, GameOver }

    private sealed class Gem
    {
        public int X;
        public int Y;
        public Project? Project;
        public bool Taken;
    }

    private sealed class Bug
    {
        public double X;
        public double Y;
        public double Min;
        public double Max;
        public double Speed;
        public string Name = "";
        public bool Caught;
        public bool Flies;      // bobs through the air and has to be netted mid-jump
        public bool Blinks;     // a Heisenbug: gone whenever it would be easy to catch
        public double BaseY;
    }

    private enum Boost { Coffee, Duck }

    private sealed class Pickup
    {
        public int X;
        public int Y;
        public Boost Kind;
        public bool Taken;
    }

    private readonly char[,] _map = new char[MapW, MapH];
    private readonly List<Gem> _gems = new();
    private readonly List<Bug> _bugs = new();
    private readonly HashSet<Key> _keys = new();

    private State _state = State.Title;
    private int _lives = StartLives;
    private readonly List<Pickup> _pickups = new();
    private int _caffeine;   // frames of coffee left: faster, and jumps higher
    private int _ducked;     // frames of rubber duck left: bugs give themselves up on touch
    private int _combo;
    private int _comboFrames;

    // The boss: the Layout Cycle. It circles in front of the flag, and nothing ships until
    // it has been netted three times. Each hit makes it loop faster. The bottom of its loop
    // sweeps the ground, so standing still under it is not safe.
    private const int BossHits = 3;
    private const double BossCentreX = 98 * Tile;
    private const double BossCentreY = 11 * Tile;
    private int _bossHp = BossHits;
    private int _bossHurt;      // frames it blinks, harmless, after a hit
    private int _bossWarned;    // frames until the "cannot ship" notice may show again
    private double _bossAngle;
    private int _mercy;         // frames nothing can hurt the player, after losing a life
    private bool _checkpoint;   // the boss arena was reached: lives lost from here respawn at its door

    private Rect BossRect => new(
        BossCentreX + Math.Cos(_bossAngle) * 26 - 7,
        BossCentreY + Math.Sin(_bossAngle) * 28 - 6,
        14, 12);
    private double _x, _y, _vx, _vy;
    private bool _grounded;
    private bool _faceLeft;
    private bool _running;
    private TimeSpan? _last;
    private double _accumulator;
    private int _frame;
    private string? _toast;
    private int _toastFrames;
    private int _swing;

    public GameView(int scale)
    {
        Width = ViewW * scale;
        Height = ViewH * scale;
        Focusable = true;
        ClipToBounds = true;

        BuildLevel();
        Respawn();
    }

    /// <summary>
    /// The player asked to see the real projects from the win screen.
    /// </summary>
    public event EventHandler? ProjectsRequested;

    private void Toast(string text)
    {
        _toast = text;
        _toastFrames = 200;
    }

    private void Respawn()
    {
        // Whoever reached the boss comes back at its door, not at the start of the level.
        _x = (_checkpoint ? 92 : 3) * Tile;
        _y = 13 * Tile;
        _vx = _vy = 0;
    }

    /// <summary>
    /// A bug bit, or the player fell. Three of those and the game is over.
    /// </summary>
    private void LoseLife(string reason)
    {
        _lives--;
        _keys.Clear();
        if (_lives <= 0)
        {
            _state = State.GameOver;
            _toastFrames = 0;
            Host.PlaySound?.Invoke("error");
            return;
        }

        Host.PlaySound?.Invoke("fail");
        Toast(reason);
        Respawn();
        _mercy = 90;
    }

    /// <summary>
    /// A fresh game: every bug and gem back, three lives.
    /// </summary>
    private void Restart()
    {
        BuildLevel();
        _checkpoint = false;
        Respawn();
        _lives = StartLives;
        _caffeine = _ducked = _combo = _comboFrames = _mercy = 0;
        _bossHp = BossHits;
        _bossHurt = _bossWarned = 0;
        _bossAngle = 0;
        _swing = 0;
        _toastFrames = 0;
        _state = State.Playing;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _running = true;
        _last = null;
        Focus();
        TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnFrame);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _running = false;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        if (_state == State.Title)
            _state = State.Playing;
        else if (_state == State.GameOver)
            Restart();
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        _keys.Clear();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsGameKey(e.Key))
            return;

        e.Handled = true;
        var first = _keys.Add(e.Key);

        if (_state == State.Title && e.Key is Key.Space or Key.Enter)
        {
            _state = State.Playing;
        }
        else if (_state == State.Won && e.Key == Key.Enter)
        {
            ProjectsRequested?.Invoke(this, EventArgs.Empty);
        }
        else if (_state == State.GameOver && first && e.Key is Key.Space or Key.Enter)
        {
            Restart();
        }
        else if (_state == State.Playing && first && e.Key is Key.Space or Key.Up or Key.W && _grounded)
        {
            _vy = _caffeine > 0 ? JumpSpeed * 1.15 : JumpSpeed;
            _grounded = false;
            Host.PlaySound?.Invoke("jump");
        }
        else if (_state == State.Playing && first && e.Key is Key.X or Key.Z && _swing == 0)
        {
            _swing = SwingFrames;
        }
#if DEBUG
        else if (_state == State.Playing && first && e.Key == Key.B)
        {
            // A cheat: straight to the boss.
            _x = 92 * Tile;
            _y = 13 * Tile;
            _vx = _vy = 0;
            Toast("SKIPPED TO THE BOSS. CHEATER");
        }
#endif
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (_keys.Remove(e.Key))
            e.Handled = true;
    }

    private static bool IsGameKey(Key key)
        => key is Key.Left or Key.Right or Key.Up or Key.A or Key.D or Key.W or Key.X or Key.Z or Key.B or Key.Space or Key.Enter;

    private void OnFrame(TimeSpan now)
    {
        if (!_running)
            return;

        // Paused while nobody can see it: minimised, or the visitor left the laptop.
        if (!IsEffectivelyVisible || !Host.AtLaptop)
        {
            _last = null;
            _keys.Clear();
            TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnFrame);
            return;
        }

        var elapsed = _last is { } last ? (now - last).TotalMilliseconds : Step;
        _last = now;
        _accumulator = Math.Min(_accumulator + elapsed, Step * 5);

        while (_accumulator >= Step)
        {
            _accumulator -= Step;
            Update();
        }

        InvalidateVisual();
        TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnFrame);
    }
}
