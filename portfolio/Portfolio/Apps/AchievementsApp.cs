using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Portfolio.Pixel;
using Portfolio.Shell;

namespace Portfolio.Apps;

/// <summary>
/// The trophy cabinet: every achievement, and which ones this visitor has earned.
/// </summary>
public class AchievementsApp : DockPanel
{
    private readonly StackPanel _list = new();
    private readonly PixelText _summary = new() { Foreground = Palette.Brush(Palette.Ink), Margin = new Thickness(12, 12, 12, 9) };

    public AchievementsApp()
    {
        Width = 470;
        Height = 420;

        SetDock(_summary, Dock.Top);
        Children.Add(_summary);
        Children.Add(new Border
        {
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(0, 3, 0, 0),
            Child = new ScrollViewer { Content = _list },
        });

        Build();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Achievements.Unlocked += OnUnlocked;
        Build();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Achievements.Unlocked -= OnUnlocked;
    }

    private void OnUnlocked(Achievement achievement) => Build();

    private void Build()
    {
        _summary.Text = $"{Achievements.Count} OF {Achievements.All.Count} UNLOCKED";
        _list.Children.Clear();

        foreach (var achievement in Achievements.All)
        {
            var earned = Achievements.Has(achievement.Id);

            var icon = new PixelSprite
            {
                Rows = earned ? Art.Trophy : Art.Locked,
                PixelSize = 3,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
            };
            DockPanel.SetDock(icon, Dock.Left);

            var text = new StackPanel
            {
                Spacing = 3,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new PixelText { Text = achievement.Title, Foreground = Palette.Brush(earned ? Palette.Ink : Palette.Bevel) },
                    new TextBlock
                    {
                        Text = achievement.How,
                        FontSize = 12,
                        Foreground = Palette.Brush(earned ? Palette.InkSoft : Palette.Bevel),
                    },
                },
            };

            _list.Children.Add(new Border
            {
                Padding = new Thickness(12, 8),
                BorderBrush = Palette.Brush(Palette.PaperDark),
                BorderThickness = new Thickness(0, 0, 0, 3),
                Child = new DockPanel { Children = { icon, text } },
            });
        }
    }
}
