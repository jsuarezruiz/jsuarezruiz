using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Portfolio.Content;
using Portfolio.Pixel;

namespace Portfolio.Apps;

/// <summary>
/// Resume.doc: a word processor showing the CV as a page. The chrome is pixel art; the page
/// itself is real, readable text. The content comes from content.json.
/// </summary>
public class ResumeApp : DockPanel
{
    private static readonly Color s_desk = Color.Parse("#8E92A8");
    private static readonly Color s_rule = Color.Parse("#D9DCE6");

    public ResumeApp()
    {
        Width = 640;
        Height = 470;

        var content = PortfolioContent.Current;
        var resume = content.Resume;

        var download = new PixelButton("DOWNLOAD PDF", Palette.TitleBlue);
        download.Click += (_, _) => Open(resume.Pdf);
        var email = new PixelButton("EMAIL ME", Palette.StartGreen);
        email.Click += (_, _) => EmailRequested?.Invoke(this, EventArgs.Empty);

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(9, 9, 9, 6),
            Children = { download, email },
        };
        SetDock(toolbar, Dock.Top);
        Children.Add(toolbar);

        // A sheet of paper on a grey desk, as a word processor shows it.
        Children.Add(new Border
        {
            Background = Palette.Brush(s_desk),
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(0, 3, 0, 0),
            Child = new ScrollViewer
            {
                Content = new Border
                {
                    Margin = new Thickness(18, 15),
                    MaxWidth = 560,
                    Padding = new Thickness(30, 26),
                    Background = Brushes.White,
                    BorderBrush = Palette.Brush(Palette.Ink),
                    BorderThickness = new Thickness(0, 0, 3, 3),
                    Child = Page(content),
                },
            },
        });
    }

    /// <summary>
    /// The visitor pressed EMAIL ME; the shell opens the mail app.
    /// </summary>
    public event EventHandler? EmailRequested;

    private async void Open(string url)
    {
        if (!string.IsNullOrEmpty(url) && TopLevel.GetTopLevel(this) is { } top)
            await top.Launcher.LaunchUriAsync(new Uri(url));
    }

    private static Control Page(PortfolioContent content)
    {
        var resume = content.Resume;
        var profile = content.Profile;
        var page = new StackPanel();

        page.Children.Add(Text(profile.Name, 26, FontWeight.Bold, Palette.Ink));
        page.Children.Add(Text($"{resume.Title} · {profile.Location}", 14, FontWeight.SemiBold, Palette.TitleBlue, new Thickness(0, 2, 0, 0)));
        page.Children.Add(Text(resume.Summary, 13, FontWeight.Normal, Palette.InkSoft, new Thickness(0, 9, 0, 0)));

        Heading(page, "Experience");
        foreach (var job in resume.Experience)
        {
            var title = new DockPanel { Margin = new Thickness(0, 9, 0, 3) };
            var period = Text(job.Period, 12, FontWeight.Normal, Palette.InkSoft);
            period.VerticalAlignment = VerticalAlignment.Bottom;
            DockPanel.SetDock(period, Dock.Right);
            title.Children.Add(period);
            title.Children.Add(Text($"{job.Role}, {job.Company}", 14, FontWeight.SemiBold, Palette.Ink));
            page.Children.Add(title);
            Bullets(page, job.Points);
        }

        Heading(page, "Open source");
        Bullets(page, resume.OpenSource);
        page.Children.Add(Text(
            $"{profile.Repos} public repositories, {profile.Stars} stars and {profile.Followers} followers on GitHub.",
            12, FontWeight.Normal, Palette.InkSoft, new Thickness(0, 3, 0, 0)));

        Heading(page, "Achievements");
        Bullets(page, resume.Achievements);

        Heading(page, "Community and teaching");
        Bullets(page, resume.Communities);

        Heading(page, "Education");
        Bullets(page, resume.Education);

        Heading(page, "Skills");
        page.Children.Add(Chips(resume.Skills));

        Heading(page, "Languages");
        page.Children.Add(Chips(resume.Languages));

        return page;
    }

    private static void Heading(StackPanel page, string text)
    {
        page.Children.Add(new PixelText
        {
            Text = text,
            Foreground = Palette.Brush(Palette.TitleBlue),
            Margin = new Thickness(0, 21, 0, 6),
        });
        page.Children.Add(new Border { Height = 2, Background = Palette.Brush(s_rule) });
    }

    private static void Bullets(StackPanel page, IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            var row = new DockPanel { Margin = new Thickness(0, 3, 0, 0) };
            var dot = new Border
            {
                Width = 5,
                Height = 5,
                Margin = new Thickness(2, 7, 9, 0),
                Background = Palette.Brush(Palette.TitleBlue),
                VerticalAlignment = VerticalAlignment.Top,
            };
            DockPanel.SetDock(dot, Dock.Left);
            row.Children.Add(dot);
            row.Children.Add(Text(line, 13, FontWeight.Normal, Palette.Ink));
            page.Children.Add(row);
        }
    }

    private static Control Chips(IEnumerable<string> items)
    {
        var wrap = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
        foreach (var item in items)
        {
            wrap.Children.Add(new Border
            {
                Margin = new Thickness(0, 0, 6, 6),
                Padding = new Thickness(9, 3),
                Background = Palette.Brush(Palette.Paper),
                BorderBrush = Palette.Brush(Palette.PaperDark),
                BorderThickness = new Thickness(2),
                Child = Text(item, 12, FontWeight.SemiBold, Palette.Ink),
            });
        }
        return wrap;
    }

    private static TextBlock Text(string text, double size, FontWeight weight, Color color, Thickness margin = default) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = weight,
        LineHeight = size * 1.45,
        Margin = margin,
        TextWrapping = TextWrapping.Wrap,
        Foreground = Palette.Brush(color),
    };
}
