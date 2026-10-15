using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Portfolio.Pixel;

namespace Portfolio.Apps;

/// <summary>
/// Things that were deleted, and should probably stay that way.
/// </summary>
public class RecycleBinApp : DockPanel
{
    private sealed record Item(string Name, string Deleted, string Note);

    private static readonly Item[] s_items =
    {
        new("final_FINAL_v2_really_final.xaml", "2019", "There was a v3."),
        new("TODO_fix_later.txt", "2014", "It is still later."),
        new("works_on_my_machine.cert", "2021", "Expired on every other machine."),
        new("CustomRenderers.zip", "2022", "Handlers took the job."),
        new("bin and obj", "today, 6 times", "Delete and rebuild. It always works."),
        new("designer_spec_1px_off.png", "2023", "The designer noticed."),
        new("margin_or_padding.md", "2017", "Tried both. Kept both."),
        new("sleep_schedule.ics", "every conference season", "Could not be recovered."),
        new("AppDelegate_backup (3).cs", "2018", "Nobody knows what backup (2) had."),
    };

    private readonly PixelText _status = new()
    {
        Text = $"{s_items.Length} ITEMS",
        Foreground = Palette.Brush(Palette.InkSoft),
        VerticalAlignment = VerticalAlignment.Center,
    };

    private readonly StackPanel _list = new();
    private int _selected = -1;

    public RecycleBinApp()
    {
        Width = 520;
        Height = 380;

        var restore = new PixelButton("RESTORE", Palette.TitleBlue);
        restore.Click += (_, _) => Say(_selected < 0
            ? "SELECT SOMETHING FIRST"
            : "CANNOT RESTORE: SOME THINGS SHOULD STAY DELETED");

        var empty = new PixelButton("EMPTY BIN", Palette.CloseRed);
        empty.Click += (_, _) =>
        {
            Say("ACCESS DENIED: SENTIMENTAL VALUE");
            Portfolio.Shell.Achievements.Unlock("hoarder");
        };

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(9, 9, 9, 6),
            Children = { restore, empty },
        };
        SetDock(toolbar, Dock.Top);
        Children.Add(toolbar);

        var footer = new Border
        {
            Padding = new Thickness(9, 6),
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(0, 3, 0, 0),
            Child = _status,
        };
        SetDock(footer, Dock.Bottom);
        Children.Add(footer);

        Children.Add(new Border
        {
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(0, 3, 0, 0),
            Child = new ScrollViewer { Content = _list },
        });

        Build();
    }

    private void Say(string text)
    {
        _status.Foreground = Palette.Brush(Palette.CloseRed);
        _status.Text = text;
    }

    private void Build()
    {
        _list.Children.Clear();
        for (var i = 0; i < s_items.Length; i++)
        {
            var index = i;
            var item = s_items[i];
            var selected = i == _selected;
            var ink = selected ? Palette.White : Palette.Ink;
            var soft = selected ? Palette.White : Palette.InkSoft;

            var text = new StackPanel
            {
                Spacing = 2,
                Children =
                {
                    new TextBlock { Text = item.Name, FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = Palette.Brush(ink) },
                    new TextBlock { Text = $"Deleted {item.Deleted}. {item.Note}", FontSize = 12, Foreground = Palette.Brush(soft), TextWrapping = TextWrapping.Wrap },
                },
            };

            var row = new Border
            {
                Padding = new Thickness(12, 6),
                Background = selected ? Palette.Brush(Palette.TitleBlue) : Brushes.Transparent,
                BorderBrush = Palette.Brush(Palette.PaperDark),
                BorderThickness = new Thickness(0, 0, 0, 3),
                Cursor = new Cursor(StandardCursorType.Hand),
                Child = text,
            };
            row.Tapped += (_, _) =>
            {
                _selected = index;
                _status.Foreground = Palette.Brush(Palette.InkSoft);
                _status.Text = "1 ITEM SELECTED";
                Build();
            };
            _list.Children.Add(row);
        }
    }
}
