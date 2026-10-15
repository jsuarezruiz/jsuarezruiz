#if DEBUG
using System;
using System.Collections.Generic;
using Avalonia.Input;
using Portfolio.Hosting;
using Portfolio.Shell;

namespace Portfolio.Game;

public partial class GameView
{
    /// <summary>
    /// Plays the boss fight with a simple bot, without drawing anything, and reports what
    /// happened frame by frame: the locked flag, each hit, any life lost, and how it ended.
    /// </summary>
    public static string RunBossTest()
    {
        // The test must not leave marks: no sounds, no trophy in the room, no achievements.
        var (sound, trophy) = (Host.PlaySound, Host.Trophy);
        Host.PlaySound = null;
        Host.Trophy = null;
        Achievements.Muted = true;
        try
        {
            var game = new GameView(2) { _lives = MaxLives, _state = State.Playing };
            game._x = 92 * Tile;
            game._y = 13 * Tile;

            var log = new List<string>();
            var (hp, lives) = (game._bossHp, game._lives);

            void Frame(int frame)
            {
                game.Update();
                if (game._bossHp != hp)
                {
                    hp = game._bossHp;
                    log.Add($"frame {frame}: boss hit, {hp} left. \"{game._toast}\"");
                }
                if (game._lives != lives)
                {
                    lives = game._lives;
                    log.Add($"frame {frame}: lost a life, {lives} left. \"{game._toast}\"");
                }
            }

            // 1. A player the boss cannot hurt walks at the flag: it must still hold them back.
            var runner = new GameView(2) { _lives = MaxLives, _state = State.Playing, _mercy = int.MaxValue };
            runner._x = 92 * Tile;
            runner._y = 13 * Tile;
            for (var step = 0; step < 150; step++)
            {
                runner._keys.Clear();
                runner._keys.Add(Key.Right);
                runner.Update();
            }
            log.Add(runner._state == State.Playing && runner._x < 104 * Tile
                ? $"flag locked: stopped at x={runner._x / Tile:0.0} tiles. \"{runner._toast}\""
                : $"FLAG NOT LOCKED: state {runner._state}, x={runner._x / Tile:0.0}");
            var frame = 0;

            // 2. Fight: stand under the middle of the loop, jump and swing as the boss comes round.
            var home = BossCentreX - PlayerW / 2;
            for (; frame < 60 * 120 && game._state == State.Playing && game._bossHp > 0; frame++)
            {
                game._keys.Clear();
                if (game._x < home - 2)
                    game._keys.Add(Key.Right);
                else if (game._x > home + 2)
                    game._keys.Add(Key.Left);

                var boss = game.BossRect;
                var dx = boss.Center.X - (game._x + PlayerW / 2);
                if (game._bossHurt == 0 && Math.Abs(dx) < 16 && boss.Bottom > game._y - 36)
                {
                    if (game._grounded)
                    {
                        game._vy = JumpSpeed;
                        game._grounded = false;
                    }
                    if (game._swing == 0)
                    {
                        game._faceLeft = dx < 0;
                        game._swing = SwingFrames;
                    }
                }
                Frame(frame);
            }

            // 3. With the boss gone, the flag must let the player through.
            for (var walked = 0; walked < 400 && game._state == State.Playing; walked++, frame++)
            {
                game._keys.Clear();
                game._keys.Add(Key.Right);
                Frame(frame);
            }

            log.Add($"ended after {frame} frames ({frame / 60.0:0.0} s): {game._state}, {game._lives} lives, boss hp {game._bossHp}");

            // 4. A second player who stands under the loop and does nothing: the boss must hurt them.
            var idle = new GameView(2) { _lives = MaxLives, _state = State.Playing };
            idle._x = home;
            idle._y = 13 * Tile;
            var idleFrame = 0;
            for (; idleFrame < 600 && idle._lives == 3; idleFrame++)
                idle.Update();
            log.Add(idle._lives < 3
                ? $"standing still: lost a life at frame {idleFrame}. \"{idle._toast}\""
                : "STANDING STILL IS SAFE: no life lost in 600 frames");
            return string.Join("\n", log);
        }
        finally
        {
            Host.PlaySound = sound;
            Host.Trophy = trophy;
            Achievements.Muted = false;
        }
    }
}
#endif
