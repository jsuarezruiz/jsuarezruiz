using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Portfolio.Content;
using Portfolio.Hosting;

namespace Portfolio.Phone;

/// <summary>
/// The phone on the desk: a home screen whose apps are the live WebAssembly builds of the
/// Avalonia projects. Unlike the laptop's pixel OS this is drawn smooth.
/// </summary>
public class PhoneShell : Grid
{
    private static readonly Color[] s_tiles =
    {
        Color.Parse("#7B4FD6"), Color.Parse("#1FB5A0"), Color.Parse("#E5484D"), Color.Parse("#3498DB"),
        Color.Parse("#F2A45C"), Color.Parse("#3DA83D"), Color.Parse("#D6409F"), Color.Parse("#5B6CFF"),
    };

    private readonly ContentControl _screen = new();
    private readonly TextBlock _clock = new() { FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = Brushes.White };
    private readonly DispatcherTimer _clockTimer = new() { Interval = TimeSpan.FromSeconds(20) };
    private readonly Control _home;
    private readonly PhoneSurprises _surprises = new();

    public PhoneShell()
    {
        RowDefinitions = new RowDefinitions("Auto,*,Auto");
        Background = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops = { new GradientStop(Color.Parse("#2B1F5C"), 0), new GradientStop(Color.Parse("#0F2A4A"), 1) },
        };

        var status = new Grid { Margin = new Thickness(22, 12, 22, 6) };
        status.Children.Add(_clock);
        status.Children.Add(Indicators());
        Children.Add(status);

        SetRow(_screen, 1);
        Children.Add(_screen);

        var indicator = new Border
        {
            Width = 110,
            Height = 5,
            CornerRadius = new CornerRadius(3),
            Background = Brushes.White,
            Opacity = 0.85,
        };
        var bar = new Border
        {
            Padding = new Thickness(0, 10, 0, 12),
            Child = indicator,
        };
        SetRow(bar, 2);
        Children.Add(bar);

        _home = Home();
        _screen.Content = _home;

        // Notifications and the incoming call are drawn over everything else.
        SetRowSpan(_surprises, 3);
        Children.Add(_surprises);

        _clockTimer.Tick += (_, _) => UpdateClock();
        UpdateClock();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _clockTimer.Start();
        UpdateClock();
        _surprises.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _clockTimer.Stop();
        _surprises.Stop();
    }

    private void UpdateClock() => _clock.Text = DateTime.Now.ToString("HH:mm");

    private Control Home()
    {
        var grid = new UniformGrid { Columns = 3, Margin = new Thickness(14, 18, 14, 0), VerticalAlignment = VerticalAlignment.Top };
        var apps = PortfolioContent.Current.Apps;
        for (var i = 0; i < apps.Count; i++)
        {
            var app = apps[i];
            var cell = new StackPanel
            {
                Spacing = 6,
                Margin = new Thickness(0, 0, 0, 20),
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand),
                Children =
                {
                    Tile(app.Name, s_tiles[i % s_tiles.Length], 58),
                    new TextBlock
                    {
                        Text = Label(app.Name),
                        FontSize = 10,
                        Foreground = Brushes.White,
                        TextAlignment = TextAlignment.Center,
                        MaxWidth = 92,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                },
            };
            cell.Tapped += (_, _) => Launch(app);
            grid.Children.Add(cell);
        }
        return grid;
    }

    /// <summary>
    /// Opens the real, running app: on the phone's screen in the room, else in the browser.
    /// </summary>
    private async void Launch(PhoneApp app)
    {
        if (Host.OpenPhoneApp is { } open)
            open(app.Url);
        else if (TopLevel.GetTopLevel(this) is { } top)
            await top.Launcher.LaunchUriAsync(new Uri(app.Url));
    }

    private static Border Tile(string name, Color color, double size) => new()
    {
        Width = size,
        Height = size,
        CornerRadius = new CornerRadius(size * 0.26),
        Background = new SolidColorBrush(color),
        HorizontalAlignment = HorizontalAlignment.Center,
        Child = new TextBlock
        {
            Text = Initials(name),
            FontSize = size * 0.36,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        },
    };

    /// <summary>
    /// The right of the status bar: signal bars, network and battery.
    /// </summary>
    private static Control Indicators()
    {
        var bars = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        for (var i = 0; i < 4; i++)
        {
            bars.Children.Add(new Border
            {
                Width = 3,
                Height = 4 + i * 2.5,
                CornerRadius = new CornerRadius(1),
                Background = Brushes.White,
                VerticalAlignment = VerticalAlignment.Bottom,
            });
        }

        var battery = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 1,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Border
                {
                    Width = 24,
                    Height = 12,
                    CornerRadius = new CornerRadius(3.5),
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(1.5),
                    Child = new Border
                    {
                        Width = 14,
                        CornerRadius = new CornerRadius(1.5),
                        Background = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    },
                },
                new Border { Width = 2, Height = 5, CornerRadius = new CornerRadius(0, 1, 1, 0), Background = Brushes.White, VerticalAlignment = VerticalAlignment.Center },
            },
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Opacity = 0.9,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                bars,
                new TextBlock { Text = "5G", FontSize = 12, FontWeight = FontWeight.SemiBold, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center },
                battery,
            },
        };
    }

    /// <summary>
    /// A long repository name on two lines, broken before its last part.
    /// </summary>
    private static string Label(string name)
    {
        var cut = name.LastIndexOfAny(s_separators);
        return name.Length > 14 && cut > 0 ? name[..(cut + 1)] + "\n" + name[(cut + 1)..] : name;
    }

    private static readonly char[] s_separators = { '.', '_', ' ' };

    /// <summary>
    /// Two letters from the part of the name that tells the apps apart.
    /// </summary>
    private static string Initials(string name)
    {
        var part = name.Split(s_separators, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(p => !p.Equals("Nova", StringComparison.OrdinalIgnoreCase) && !p.Equals("Avalonia", StringComparison.OrdinalIgnoreCase)) ?? name;
        return part[..Math.Min(2, part.Length)].ToUpperInvariant();
    }
}
