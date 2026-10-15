using System.Linq;
using Portfolio.Content;

namespace Portfolio.Game;

public partial class GameView
{
    private void BuildLevel()
    {
        _bugs.Clear();
        _gems.Clear();
        for (var x = 0; x < MapW; x++)
            for (var y = 0; y < MapH; y++)
                _map[x, y] = y >= 15 ? '#' : ' ';

        foreach (var (from, to) in new[] { (22, 24), (45, 48), (70, 72), (88, 90) })
            for (var x = from; x <= to; x++)
                _map[x, 15] = _map[x, 16] = ' ';

        // Platforms are window title bars: the UI is the level.
        foreach (var (y, from, to) in new[]
                 {
                     (11, 10, 14), (9, 17, 20), (11, 27, 31), (11, 45, 48), (10, 54, 58),
                     (7, 60, 63), (11, 66, 68), (11, 76, 80), (8, 82, 85), (11, 92, 95),
                 })
        {
            for (var x = from; x <= to; x++)
                _map[x, y] = '=';
        }

        // Bugs patrol a stretch of ground or the top of a title bar.
        var patrols = new[]
        {
            (Row: 10, From: 10, To: 14), (Row: 14, From: 27, To: 40), (Row: 9, From: 54, To: 58),
            (Row: 14, From: 60, To: 68), (Row: 10, From: 76, To: 80), (Row: 14, From: 49, To: 58),
            (Row: 7, From: 82, To: 85),
        };
        for (var i = 0; i < patrols.Length; i++)
        {
            var (row, from, to) = patrols[i];
            _bugs.Add(new Bug
            {
                X = (from + to) / 2.0 * Tile,
                Y = row * Tile,
                Min = from * Tile,
                Max = to * Tile,
                Speed = i % 2 == 0 ? 0.35 : -0.5,
                Name = s_bugNames[i % s_bugNames.Length],
            });
        }

        // The ones that fly. They bob up and down, so they have to be netted mid-jump.
        var flyers = new[]
        {
            (Row: 8, From: 31, To: 43, Speed: 0.6, Name: "HEISENBUG", Blinks: true),
            (Row: 6, From: 63, To: 74, Speed: -0.8, Name: "FLAKY TEST", Blinks: false),
            (Row: 5, From: 74, To: 87, Speed: 1.2, Name: "RACE CONDITION", Blinks: false),
        };
        foreach (var (row, from, to, speed, name, blinks) in flyers)
        {
            _bugs.Add(new Bug
            {
                X = (from + to) / 2.0 * Tile,
                Y = row * Tile,
                BaseY = row * Tile,
                Min = from * Tile,
                Max = to * Tile,
                Speed = speed,
                Name = name,
                Flies = true,
                Blinks = blinks,
            });
        }

        // Help along the way: a coffee, and a rubber duck.
        _pickups.Clear();
        _pickups.Add(new Pickup { X = 29, Y = 9, Kind = Boost.Coffee });
        _pickups.Add(new Pickup { X = 67, Y = 9, Kind = Boost.Duck });

        var projects = PortfolioContent.Current.ByEra("xamarin").Take(5).ToList();
        var spots = new[] { (12, 9), (18, 7), (47, 9), (61, 5), (83, 6) };
        for (var i = 0; i < spots.Length; i++)
        {
            _gems.Add(new Gem
            {
                X = spots[i].Item1,
                Y = spots[i].Item2,
                Project = i < projects.Count ? projects[i] : null,
            });
        }
    }
}
