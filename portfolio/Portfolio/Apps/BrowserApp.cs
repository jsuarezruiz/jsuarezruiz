using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Portfolio.Content;
using Portfolio.Pixel;

namespace Portfolio.Apps;

/// <summary>
/// A web browser open on the GitHub repository list. GitHub forbids being embedded in another
/// page, so the list is drawn here, laid out the way GitHub lays it out. It starts from
/// content.json and is replaced by the full, live list from GitHub's public API.
/// The page arrives the way pages did over dial-up: as coarse blocks that sharpen in steps.
/// </summary>
public class BrowserApp : DockPanel
{
    private sealed record Repo(string Name, string Description, string Url, int Stars, string Language, DateTimeOffset? Pushed);

    // How many screen pixels each block covers at each step of the reveal.
    private static readonly int[] s_blocks = { 24, 14, 8, 4, 2 };

    // GitHub's own colours for the languages that show up here.
    private static readonly Dictionary<string, string> s_languages = new()
    {
        ["C#"] = "#178600",
        ["JavaScript"] = "#F1E05A",
        ["TypeScript"] = "#3178C6",
        ["HTML"] = "#E34C26",
        ["CSS"] = "#663399",
        ["Python"] = "#3572A5",
        ["Java"] = "#B07219",
        ["Kotlin"] = "#A97BFF",
        ["Swift"] = "#F05138",
        ["Dart"] = "#00B4AB",
        ["PowerShell"] = "#012456",
        ["Shell"] = "#89E051",
        ["F#"] = "#B845FC",
        ["Objective-C"] = "#438EFF",
    };

    private static readonly Color s_line = Color.Parse("#D8DEE4");
    private static readonly Color s_link = Color.Parse("#0969DA");
    private static readonly Color s_muted = Color.Parse("#59636E");

    private readonly Border _viewport;
    private readonly StackPanel _list = new();
    private readonly TextBlock _count = new() { FontSize = 12, FontWeight = FontWeight.SemiBold };
    private readonly BlockView _blocks = new() { IsVisible = false, IsHitTestVisible = false };
    private readonly PixelText _status = new() { Foreground = Palette.Brush(Palette.InkSoft), VerticalAlignment = VerticalAlignment.Center };
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(230) };
    private readonly PixelButton _byStars = new("STARS", Palette.Amber, Palette.Ink);
    private readonly PixelButton _byDate = new("UPDATED", Palette.PaperDark, Palette.Ink);
    private readonly string _address;
    private List<Repo> _repos;
    private bool _sortByDate;
    private bool _live;
    private int _step;

    public BrowserApp()
    {
        Width = 680;
        Height = 470;

        var content = PortfolioContent.Current;
        _address = content.Profile.Url + "?tab=repositories";
        _repos = content.Projects.Select(p => new Repo(p.Name, p.Description, p.Url, p.Stars, "C#", null)).ToList();

        var reload = new PixelButton("RELOAD", Palette.PaperDark, Palette.Ink);
        reload.Click += (_, _) => { Reveal(); _ = LoadLive(); };
        var real = new PixelButton("OPEN GITHUB", Palette.TitleBlue);
        real.Click += (_, _) => Open(_address);

        var address = new Border
        {
            Margin = new Thickness(6, 0),
            Padding = new Thickness(9, 0),
            Background = Brushes.White,
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(3),
            Child = new TextBlock
            {
                Text = _address,
                FontSize = 12,
                Foreground = Palette.Brush(Palette.Ink),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            },
        };

        var toolbar = new DockPanel { Margin = new Thickness(9, 9, 9, 6) };
        SetDock(reload, Dock.Left);
        SetDock(real, Dock.Right);
        toolbar.Children.Add(reload);
        toolbar.Children.Add(real);
        toolbar.Children.Add(address);
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

        _byStars.Click += (_, _) => Sort(byDate: false);
        _byDate.Click += (_, _) => Sort(byDate: true);

        _viewport = new Border
        {
            Background = Brushes.White,
            Child = new ScrollViewer { Content = Page(content) },
        };

        Children.Add(new Border
        {
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(0, 3, 0, 0),
            Child = new Grid { Children = { _viewport, _blocks } },
        });

        _timer.Tick += (_, _) => NextStep();
        ShowRepos();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        // Wait for the first layout, so there is a page to take the picture of.
        Dispatcher.UIThread.Post(Reveal, DispatcherPriority.Background);
        _ = LoadLive();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    // Live data

    /// <summary>
    /// Fetches every public, non-fork repository from GitHub's API.
    /// </summary>
    private async Task LoadLive()
    {
        try
        {
            using var http = new HttpClient();
            // A browser sends its own User-Agent and rejects ours; outside one, GitHub requires it.
            if (!OperatingSystem.IsBrowser())
                http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "portfolio");

            var login = PortfolioContent.Current.Profile.Login;
            var repos = new List<Repo>();
            for (var page = 1; page <= 10; page++)
            {
                var json = await http.GetStringAsync($"https://api.github.com/users/{login}/repos?per_page=100&page={page}");
                using var document = JsonDocument.Parse(json);
                var count = 0;
                foreach (var item in document.RootElement.EnumerateArray())
                {
                    count++;
                    if (item.GetProperty("fork").GetBoolean())
                        continue;
                    repos.Add(new Repo(
                        item.GetProperty("name").GetString() ?? "",
                        Str(item, "description"),
                        item.GetProperty("html_url").GetString() ?? "",
                        item.GetProperty("stargazers_count").GetInt32(),
                        Str(item, "language"),
                        item.TryGetProperty("pushed_at", out var pushed) && pushed.ValueKind == JsonValueKind.String
                            ? pushed.GetDateTimeOffset()
                            : null));
                }
                if (count < 100)
                    break;
            }

            _repos = repos;
            _live = true;
            ShowRepos();
        }
        catch (Exception ex)
        {
            // Offline, or over GitHub's hourly limit: the saved list stays.
            _live = false;
            Console.Error.WriteLine("GitHub list: " + ex);
        }

        if (!_timer.IsEnabled)
            _status.Text = Done();
    }

    private static string Str(JsonElement item, string name)
        => item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : "";

    private string Done() => _live ? "DONE. LIVE FROM GITHUB" : "DONE. SAVED COPY";

    // Pixel reveal

    private void Reveal()
    {
        _step = 0;
        _status.Text = "CONNECTING AT 56K...";
        NextStep();
        _timer.Start();
    }

    private void NextStep()
    {
        var size = _viewport.Bounds.Size;
        if (_step >= s_blocks.Length || size.Width < 1 || size.Height < 1)
        {
            _timer.Stop();
            _blocks.IsVisible = false;
            _status.Text = Done();
            return;
        }

        // Draw the page into a bitmap 1/block of its size, then stretch it back unsmoothed.
        var block = s_blocks[_step++];
        var pixels = new PixelSize(Math.Max(1, (int)(size.Width / block)), Math.Max(1, (int)(size.Height / block)));
        var bitmap = new RenderTargetBitmap(pixels, new Vector(96.0 / block, 96.0 / block));
        bitmap.Render(_viewport);

        _blocks.Show(bitmap);
        _blocks.IsVisible = true;
        _status.Text = $"LOADING... {_step * 100 / (s_blocks.Length + 1)}%";
    }

    /// <summary>
    /// Draws a small bitmap stretched over its whole area, with hard-edged pixels.
    /// </summary>
    private sealed class BlockView : Control
    {
        private Bitmap? _source;

        public void Show(Bitmap source)
        {
            _source?.Dispose();
            _source = source;
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            if (_source is null)
                return;
            using var _ = context.PushRenderOptions(new RenderOptions { BitmapInterpolationMode = BitmapInterpolationMode.None });
            context.DrawImage(_source, new Rect(_source.Size), new Rect(Bounds.Size));
        }
    }

    // The page

    private async void Open(string url)
    {
        if (TopLevel.GetTopLevel(this) is { } top)
            await top.Launcher.LaunchUriAsync(new Uri(url));
    }

    private Control Page(PortfolioContent content)
    {
        var profile = content.Profile;
        var page = new StackPanel { Margin = new Thickness(24, 18) };

        var avatar = new Border
        {
            Width = 72,
            Height = 72,
            CornerRadius = new CornerRadius(36),
            ClipToBounds = true,
            Background = Palette.Brush(Color.Parse("#DCE6FB")),
            Child = new PixelSprite
            {
                Rows = Art.Player,
                PixelSize = 5,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

        var who = new StackPanel
        {
            Margin = new Thickness(15, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                Text(profile.Name, 20, FontWeight.Bold, Palette.Ink),
                Text(profile.Login, 14, FontWeight.Normal, s_muted),
                Text(profile.Bio, 13, FontWeight.Normal, Palette.Ink, new Thickness(0, 6, 0, 0)),
                Text($"{profile.Followers} followers · {profile.Location}", 12, FontWeight.Normal, s_muted, new Thickness(0, 4, 0, 0)),
            },
        };

        var header = new DockPanel();
        SetDock(avatar, Dock.Left);
        header.Children.Add(avatar);
        header.Children.Add(who);
        page.Children.Add(header);

        // GitHub's tab strip, with Repositories selected.
        var tabs = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 21, Margin = new Thickness(0, 18, 0, 0) };
        tabs.Children.Add(Tab("Overview", null, selected: false));
        tabs.Children.Add(Tab("Repositories", _count, selected: true));
        tabs.Children.Add(Tab("Stars", null, selected: false));
        page.Children.Add(tabs);
        page.Children.Add(new Border { Height = 1, Background = Palette.Brush(s_line) });

        var sort = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(0, 12, 0, 6),
            Children =
            {
                new PixelText { Text = "SORT BY", Foreground = Palette.Brush(Palette.InkSoft), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) },
                _byStars,
                _byDate,
            },
        };
        page.Children.Add(sort);
        page.Children.Add(_list);

        return page;
    }

    private static Control Tab(string label, Control? badge, bool selected)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new Thickness(0, 0, 0, 8) };
        row.Children.Add(Text(label, 13, selected ? FontWeight.SemiBold : FontWeight.Normal, Palette.Ink));
        if (badge is not null)
        {
            row.Children.Add(new Border
            {
                Padding = new Thickness(7, 1),
                CornerRadius = new CornerRadius(10),
                Background = Palette.Brush(Color.Parse("#E6EAEF")),
                Child = badge,
            });
        }

        return new Border
        {
            Child = row,
            BorderBrush = Palette.Brush(Color.Parse("#FD8C73")),
            BorderThickness = new Thickness(0, 0, 0, selected ? 2 : 0),
        };
    }

    private void Sort(bool byDate)
    {
        _sortByDate = byDate;
        _byStars.Face = byDate ? Palette.PaperDark : Palette.Amber;
        _byDate.Face = byDate ? Palette.Amber : Palette.PaperDark;
        ShowRepos();
    }

    private void ShowRepos()
    {
        var ordered = _sortByDate
            ? _repos.OrderByDescending(r => r.Pushed ?? DateTimeOffset.MinValue).ThenByDescending(r => r.Stars)
            : _repos.OrderByDescending(r => r.Stars).ThenBy(r => r.Name);

        _count.Text = _live ? _repos.Count.ToString() : PortfolioContent.Current.Profile.Repos.ToString();
        _list.Children.Clear();
        foreach (var repo in ordered)
            _list.Children.Add(Row(repo));
    }

    private Control Row(Repo repo)
    {
        var name = Text(repo.Name, 16, FontWeight.SemiBold, s_link);
        name.Cursor = new Cursor(StandardCursorType.Hand);
        name.Tapped += (_, _) => Open(repo.Url);

        var title = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 9,
            Children =
            {
                name,
                new Border
                {
                    Padding = new Thickness(7, 0),
                    CornerRadius = new CornerRadius(10),
                    BorderBrush = Palette.Brush(s_line),
                    BorderThickness = new Thickness(1),
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = Text("Public", 11, FontWeight.SemiBold, s_muted),
                },
            },
        };

        var meta = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 15, Margin = new Thickness(0, 9, 0, 0) };
        if (repo.Language.Length > 0)
        {
            meta.Children.Add(new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Children =
                {
                    new Border
                    {
                        Width = 11,
                        Height = 11,
                        CornerRadius = new CornerRadius(6),
                        VerticalAlignment = VerticalAlignment.Center,
                        Background = Palette.Brush(Color.Parse(s_languages.TryGetValue(repo.Language, out var hex) ? hex : "#8B949E")),
                    },
                    Text(repo.Language, 12, FontWeight.Normal, s_muted),
                },
            });
        }
        meta.Children.Add(new PixelText { Text = $"* {repo.Stars}", Foreground = Palette.Brush(Palette.Amber), VerticalAlignment = VerticalAlignment.Center });
        if (repo.Pushed is { } pushed)
            meta.Children.Add(Text("Updated " + Ago(pushed), 12, FontWeight.Normal, s_muted));

        var body = new StackPanel { Children = { title } };
        if (repo.Description.Length > 0)
            body.Children.Add(Text(repo.Description, 13, FontWeight.Normal, s_muted, new Thickness(0, 5, 0, 0)));
        body.Children.Add(meta);

        return new Border
        {
            Padding = new Thickness(0, 18),
            BorderBrush = Palette.Brush(s_line),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = body,
        };
    }

    private static string Ago(DateTimeOffset when)
    {
        var days = (DateTimeOffset.UtcNow - when).TotalDays;
        if (days < 1)
            return "today";
        if (days < 2)
            return "yesterday";
        if (days < 30)
            return $"{(int)days} days ago";
        return "on " + when.ToString(when.Year == DateTimeOffset.UtcNow.Year ? "MMM d" : "MMM d, yyyy");
    }

    private static TextBlock Text(string text, double size, FontWeight weight, Color color, Thickness margin = default) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = weight,
        Margin = margin,
        TextWrapping = TextWrapping.Wrap,
        Foreground = Palette.Brush(color),
    };
}
