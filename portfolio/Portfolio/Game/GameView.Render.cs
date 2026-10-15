using System;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Portfolio.Pixel;

namespace Portfolio.Game;

public partial class GameView
{
    public override void Render(DrawingContext context)
    {
        // Fill whatever space the window gives, centred on black. The scale moves in half steps:
        // tiles are 8 pixels, so their edges still land on whole screen pixels and nothing shimmers.
        context.FillRectangle(Brushes.Black, new Rect(Bounds.Size));
        var scale = Math.Max(1, Math.Floor(Math.Min(Bounds.Width / ViewW, Bounds.Height / ViewH) * 2) / 2);
        var offsetX = Math.Floor((Bounds.Width - ViewW * scale) / 2);
        var offsetY = Math.Floor((Bounds.Height - ViewH * scale) / 2);
        using var _ = context.PushTransform(Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offsetX, offsetY));
        using var clip = context.PushClip(new Rect(0, 0, ViewW, ViewH));

        context.FillRectangle(s_sky, new Rect(0, 0, ViewW, ViewH));

        var camera = Math.Clamp(Math.Floor(_x - ViewW / 2.0), 0, MapW * Tile - ViewW);

        // Far hills scroll at a third of the speed.
        for (var i = 0; i < 6; i++)
        {
            var hx = i * 90 - camera / 3 % 90;
            context.FillRectangle(s_hills, new Rect(hx, 84, 60, 40));
            context.FillRectangle(s_hills, new Rect(hx + 8, 76, 44, 8));
        }

        var firstTile = (int)(camera / Tile);
        for (var tx = firstTile; tx <= firstTile + ViewW / Tile + 1 && tx < MapW; tx++)
            for (var ty = 0; ty < MapH; ty++)
            {
                var px = tx * Tile - camera;
                var py = ty * Tile;
                switch (_map[tx, ty])
                {
                    case '#':
                        context.FillRectangle(s_dirt, new Rect(px, py, Tile, Tile));
                        if (ty == 15)
                            context.FillRectangle(s_grass, new Rect(px, py, Tile, 3));
                        break;
                    case '=':
                        context.FillRectangle(s_title, new Rect(px, py, Tile, Tile));
                        context.FillRectangle(s_titleLight, new Rect(px, py, Tile, 1));
                        if (tx + 1 >= MapW || _map[tx + 1, ty] != '=')
                            context.FillRectangle(s_close, new Rect(px + 3, py + 2, 4, 4));
                        break;
                }
            }

        // The boss, circling in front of the flag.
        if (_bossHp > 0)
        {
            var boss = BossRect;
            var bx = Math.Floor(boss.X - camera) - 1;
            if (_bossHurt == 0 || _frame / 4 % 2 == 0)
                Art.Draw(context, _frame / 8 % 2 == 0 ? Art.BugFlyUp : Art.BugFlyDown, bx, Math.Floor(boss.Y) - 2, 2, Math.Cos(_bossAngle) > 0);
            for (var i = 0; i < _bossHp; i++)
                context.FillRectangle(s_close, new Rect(bx + 2 + i * 5, Math.Floor(boss.Y) - 7, 4, 3));
            PixelFont.Draw(context, "LAYOUT CYCLE", BossCentreX - camera - 35, 5 * Tile, 1, s_ink);
        }

        // Goal flag.
        var fx = 104 * Tile - camera;
        context.FillRectangle(s_pole, new Rect(fx + 3, 9 * Tile, 2, 6 * Tile));
        context.FillRectangle(s_flag, new Rect(fx + 5, 9 * Tile, 10, 7));

        foreach (var gem in _gems)
        {
            if (!gem.Taken)
                Art.Draw(context, Art.Gem, gem.X * Tile - camera, gem.Y * Tile + Math.Sin(_frame / 12.0) * 1.5, 1);
        }

        foreach (var bug in _bugs)
        {
            if (bug.Caught)
                continue;
            var sprite = !bug.Flies ? Art.Bug : _frame / 5 % 2 == 0 ? Art.BugFlyUp : Art.BugFlyDown;
            // A Heisenbug that cannot be caught right now stays on screen as a faint ghost,
            // so it reads as a trick and not as a bug that failed to draw.
            using (context.PushOpacity(Visible(bug) ? 1 : 0.22))
                Art.Draw(context, sprite, Math.Floor(bug.X - camera), Math.Floor(bug.Y), 1, bug.Speed < 0);
            if (bug.Blinks)
                PixelFont.Draw(context, Visible(bug) ? "HEISENBUG" : "NOT OBSERVED", Math.Floor(bug.X - camera) - 20, Math.Floor(bug.Y) - 10, 1, s_ink);
        }

        foreach (var pickup in _pickups)
        {
            if (!pickup.Taken)
                Art.Draw(context, pickup.Kind == Boost.Coffee ? Art.Coffee : Art.Duck,
                    pickup.X * Tile - camera, pickup.Y * Tile + Math.Sin(_frame / 10.0) * 1.5, 1);
        }

        var walking = _grounded && _vx != 0 && _frame / 6 % 2 == 0;
        var sx = Math.Floor(_x - camera) - 1;
        var sy = Math.Floor(_y);
        if (_caffeine > 0 && _frame % 4 < 2)
            sy -= 1; // too much coffee
        // Blinks while nothing can hurt it.
        if (_mercy == 0 || _frame / 4 % 2 == 0)
            Art.Draw(context, walking ? Art.PlayerStep : Art.Player, sx, sy, 1, _faceLeft);
        if (_ducked > 0 && (_ducked > 90 || _frame / 6 % 2 == 0))
            Art.Draw(context, Art.Duck, sx, sy - 9, 1, _faceLeft);

        // The net: carried upright, swung out in front while catching.
        if (_swing > 0)
        {
            var nx = _faceLeft ? sx - 10 : sx + 8;
            context.FillRectangle(s_netPole, new Rect(_faceLeft ? sx - 4 : sx + 7, sy + 5, 5, 1));
            context.FillRectangle(s_netMesh, new Rect(nx, sy, 10, 8));
            context.FillRectangle(s_netPole, new Rect(nx, sy, 10, 1));
        }
        else
        {
            var px = _faceLeft ? sx : sx + 7;
            context.FillRectangle(s_netPole, new Rect(px, sy - 3, 1, 9));
            context.FillRectangle(s_netMesh, new Rect(px - 2, sy - 7, 5, 4));
        }

        PixelFont.Draw(context, $"BUGS {_bugs.Count(b => b.Caught)}/{_bugs.Count}   GEMS {_gems.Count(g => g.Taken)}/{_gems.Count}", 4, 4, 1, s_ink);

        if (_toastFrames > 0 && _toast is not null)
        {
            var size = PixelFont.Measure(_toast, 1);
            var tx = Math.Floor((ViewW - size.Width) / 2);
            context.FillRectangle(s_shade, new Rect(tx - 4, 14, size.Width + 8, 13));
            PixelFont.Draw(context, _toast, tx, 17, 1, s_white);
        }

        // Lives, as hearts in the corner.
        if (_lives > MaxLives)
        {
            // Too many to draw one by one: a heart and a count.
            var count = "X" + _lives;
            PixelFont.Draw(context, count, ViewW - 4 - count.Length * 6, 5, 1, s_ink);
            Art.Draw(context, s_heart, ViewW - 14 - count.Length * 6, 4, 1);
        }
        else
        {
            for (var i = 0; i < MaxLives; i++)
                Art.Draw(context, i < _lives ? s_heart : s_heartLost, ViewW - 12 - i * 9, 4, 1);
        }

        if (_state == State.Title)
        {
            Banner(context, "BUG HUNT", "CATCH THEM ALL BEFORE YOU SHIP", "ARROWS MOVE  SPACE JUMP  X NET");
        }
        else if (_state == State.Won)
        {
            var left = _bugs.Count(b => !b.Caught);
            Banner(context, "SHIPPED!",
                left == 0 ? "ZERO KNOWN BUGS" : $"WITH {left} KNOWN BUGS. IT'S FINE",
                "PRESS ENTER TO SEE THE REAL CODE");
        }
        else if (_state == State.GameOver)
        {
            Banner(context, "GAME OVER", "ROLLED BACK TO THE LAST GOOD BUILD", "PRESS ENTER TO TRY AGAIN");
        }
    }

    private static void Banner(DrawingContext context, string title, string subtitle, string hint)
    {
        context.FillRectangle(s_shade, new Rect(0, 34, ViewW, 62));
        Centered(context, title, 42, 2, s_white);
        Centered(context, subtitle, 64, 1, s_flag);
        Centered(context, hint, 80, 1, s_white);
    }

    private static void Centered(DrawingContext context, string text, double y, double px, IBrush brush)
    {
        var size = PixelFont.Measure(text, px);
        PixelFont.Draw(context, text, Math.Floor((ViewW - size.Width) / 2), y, px, brush);
    }
}
