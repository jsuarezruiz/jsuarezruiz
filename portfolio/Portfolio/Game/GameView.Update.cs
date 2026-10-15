using System;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using Portfolio.Hosting;
using Portfolio.Shell;

namespace Portfolio.Game;

public partial class GameView
{
    /// <summary>
    /// Whether a blinking bug can be seen, and so caught, on this frame.
    /// </summary>
    private bool Visible(Bug bug) => !bug.Blinks || _frame / 70 % 2 == 0;

    private bool Solid(int tx, int ty)
        => tx < 0 || tx >= MapW || (ty >= 0 && ty < MapH && _map[tx, ty] != ' ');

    private bool Collides(double x, double y)
    {
        var x0 = (int)Math.Floor(x / Tile);
        var x1 = (int)Math.Floor((x + PlayerW - 0.01) / Tile);
        var y0 = (int)Math.Floor(y / Tile);
        var y1 = (int)Math.Floor((y + PlayerH - 0.01) / Tile);
        for (var tx = x0; tx <= x1; tx++)
            for (var ty = y0; ty <= y1; ty++)
            {
                if (Solid(tx, ty))
                    return true;
            }
        return false;
    }

    private void Update()
    {
        _frame++;
        if (_mercy > 0)
            _mercy--;
        if (_toastFrames > 0)
            _toastFrames--;
        if (_state != State.Playing)
            return;
        if (_swing > 0)
            _swing--;

        var left = _keys.Contains(Key.Left) || _keys.Contains(Key.A);
        var right = _keys.Contains(Key.Right) || _keys.Contains(Key.D);
        var speed = _caffeine > 0 ? RunSpeed * 1.6 : RunSpeed;
        _vx = left == right ? 0 : left ? -speed : speed;
        if (_caffeine > 0)
            _caffeine--;
        if (_ducked > 0)
            _ducked--;
        if (_comboFrames > 0 && --_comboFrames == 0)
            _combo = 0;
        if (_vx != 0)
            _faceLeft = _vx < 0;

        if (_vx != 0 && !Collides(_x + _vx, _y))
            _x += _vx;

        _vy = Math.Min(_vy + Gravity, 5);
        if (!Collides(_x, _y + _vy))
        {
            _y += _vy;
            _grounded = false;
        }
        else
        {
            // Settle against the surface one pixel at a time.
            var dir = Math.Sign(_vy);
            while (!Collides(_x, _y + dir))
                _y += dir;
            _grounded = _vy > 0;
            _vy = 0;
        }

        if (_y > MapH * Tile)
        {
            LoseLife("FELL INTO PRODUCTION");
            return;
        }

        var body = new Rect(_x, _y, PlayerW, PlayerH);

        // The net reaches a little in front of the player while it swings.
        var net = _swing > 0
            ? new Rect(_faceLeft ? _x - 11 : _x + PlayerW, _y - 3, 11, PlayerH + 3)
            : default;

        foreach (var bug in _bugs)
        {
            if (bug.Caught)
                continue;

            bug.X += bug.Speed;
            if (bug.X <= bug.Min || bug.X >= bug.Max)
                bug.Speed = -bug.Speed;
            if (bug.Flies)
                bug.Y = bug.BaseY + Math.Sin(_frame / 14.0 + bug.Min) * 9;

            // A Heisenbug is only there some of the time, and never when you reach for it.
            if (bug.Blinks && !Visible(bug))
                continue;

            var hit = new Rect(bug.X + 1, bug.Y + 2, 6, 6);
            var stomped = _vy > 0 && body.Bottom <= hit.Y + 4 && body.Intersects(hit);
            var explained = _ducked > 0 && body.Intersects(hit);
            if (stomped || explained || (_swing > 0 && net.Intersects(hit)))
            {
                bug.Caught = true;
                Achievements.Unlock("bug-catcher");
                Host.PlaySound?.Invoke("catch");

                _combo = _comboFrames > 0 ? _combo + 1 : 1;
                _comboFrames = 150;
                Toast(_combo >= 2 ? $"COMBO X{_combo}: PAIR PROGRAMMING"
                    : explained ? bug.Name + " EXPLAINED ITSELF TO THE DUCK"
                    : bug.Blinks ? "CAUGHT: HEISENBUG. IT WAS THERE ALL ALONG"
                    : "CAUGHT: " + bug.Name);
                if (stomped)
                    _vy = JumpSpeed * 0.6;
            }
            else if (_mercy == 0 && body.Intersects(hit))
            {
                LoseLife("BUILD FAILED: " + bug.Name);
                return;
            }
        }

        foreach (var gem in _gems)
        {
            if (gem.Taken || !body.Intersects(new Rect(gem.X * Tile, gem.Y * Tile, Tile, Tile)))
                continue;
            gem.Taken = true;
            Host.PlaySound?.Invoke("gem");
            Toast(gem.Project is { } p ? $"{p.Name}  * {p.Stars}" : "GEM");
        }

        foreach (var pickup in _pickups)
        {
            if (pickup.Taken || !body.Intersects(new Rect(pickup.X * Tile, pickup.Y * Tile, Tile, Tile)))
                continue;
            pickup.Taken = true;
            Host.PlaySound?.Invoke("gem");
            if (pickup.Kind == Boost.Coffee)
            {
                _caffeine = 420;
                Toast("COFFEE! SHIPPING AT 1.6X");
            }
            else
            {
                _ducked = 420;
                Toast("RUBBER DUCK! BUGS EXPLAIN THEMSELVES");
            }
        }

        if (_bossHp > 0)
        {
            _bossAngle += 0.03 + (BossHits - _bossHp) * 0.022;
            if (_x >= 92 * Tile)
                _checkpoint = true;
            if (_bossHurt > 0)
                _bossHurt--;
            if (_bossWarned > 0)
                _bossWarned--;

            var boss = BossRect;
            var stomp = _vy > 0 && body.Bottom <= boss.Y + 5 && body.Intersects(boss);
            if (_bossHurt == 0 && (stomp || (_ducked > 0 && body.Intersects(boss)) || (_swing > 0 && net.Intersects(boss))))
            {
                _bossHp--;
                _bossHurt = 70;
                Host.PlaySound?.Invoke("catch");
                if (stomp)
                    _vy = JumpSpeed * 0.7;
                if (_bossHp == 0)
                {
                    Toast("LAYOUT CYCLE BROKEN. YOU MAY SHIP");
                    Achievements.Unlock("loop-breaker");
                    Host.PlaySound?.Invoke("achievement");
                }
                else
                {
                    Toast(_bossHp == 2 ? "MEASURE INVALIDATED. IT LOOPS FASTER" : "ARRANGE INVALIDATED. ONE MORE");
                }
            }
            else if (_bossHurt == 0 && _mercy == 0 && body.Intersects(boss))
            {
                LoseLife("STACK OVERFLOW: LAYOUT CYCLE");
                return;
            }

            // The flag stays out of reach while the cycle is running.
            if (_x >= 103 * Tile)
            {
                _x = 103 * Tile - 1;
                if (_bossWarned == 0)
                {
                    _bossWarned = 180;
                    Toast("CANNOT SHIP: LAYOUT CYCLE DETECTED");
                }
            }
        }

        if (_x >= 104 * Tile)
        {
            _state = State.Won;
            Achievements.Unlock("shipped");
            Host.PlaySound?.Invoke("chime");
            if (_bugs.All(b => b.Caught))
                Achievements.Unlock("zero-bugs");
            _keys.Clear();
            Host.Trophy?.Invoke();
        }
    }
}
