using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Portfolio.Content;
using Portfolio.Pixel;

namespace Portfolio.Apps;

/// <summary>
/// Repositories grouped by era, with stars pulled from content.json.
/// </summary>
public class ProjectsApp : DockPanel
{
    private readonly StackPanel _list = new() { Spacing = 6, Margin = new Thickness(9) };
    private readonly StackPanel _tabs = new() { Orientation = Orientation.Horizontal, Spacing = 6 };

    public ProjectsApp()
    {
        Width = 560;
        Height = 390;

        var content = PortfolioContent.Current;

        var header = new StackPanel { Spacing = 9, Margin = new Thickness(9, 9, 9, 3) };
        header.Children.Add(new PixelText
        {
            Text = $"{content.Profile.Repos} REPOS   * {content.Profile.Stars} STARS   {content.Profile.Followers} FOLLOWERS",
            Foreground = Palette.Brush(Palette.InkSoft),
        });
        header.Children.Add(_tabs);
        SetDock(header, Dock.Top);
        Children.Add(header);

        Children.Add(new ScrollViewer { Content = _list });

        foreach (var era in content.Eras)
        {
            var tab = new PixelButton(era.Name.ToUpperInvariant(), Palette.PaperDark, Palette.Ink) { Tag = era };
            tab.Click += (_, _) => Show(era);
            _tabs.Children.Add(tab);
        }

        if (content.Eras.Count > 0)
            Show(content.Eras[^1]);
    }

    private void Show(Era era)
    {
        foreach (var child in _tabs.Children)
        {
            if (child is PixelButton tab)
                tab.Face = ReferenceEquals(tab.Tag, era) ? Palette.Amber : Palette.PaperDark;
        }

        _list.Children.Clear();
        foreach (var project in PortfolioContent.Current.ByEra(era.Id))
            _list.Children.Add(Row(project));
    }

    private Control Row(Project project)
    {
        var open = new PixelButton("OPEN", Palette.TitleBlue) { VerticalAlignment = VerticalAlignment.Center };
        open.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is { } top)
                await top.Launcher.LaunchUriAsync(new Uri(project.Url));
        };

        var title = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 9 };
        title.Children.Add(new TextBlock
        {
            Text = project.Name,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = Palette.Brush(Palette.Ink),
        });
        title.Children.Add(new PixelText
        {
            Text = $"* {project.Stars}",
            Foreground = Palette.Brush(Palette.Amber),
            VerticalAlignment = VerticalAlignment.Center,
        });

        var text = new StackPanel { Spacing = 3 };
        text.Children.Add(title);
        text.Children.Add(new TextBlock
        {
            Text = project.Description,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Palette.Brush(Palette.InkSoft),
        });

        var row = new DockPanel();
        SetDock(open, Dock.Right);
        row.Children.Add(open);
        row.Children.Add(text);

        return new Border
        {
            Padding = new Thickness(9, 6),
            BorderThickness = new Thickness(0, 0, 0, 3),
            BorderBrush = Palette.Brush(Palette.PaperDark),
            Child = row,
        };
    }
}
