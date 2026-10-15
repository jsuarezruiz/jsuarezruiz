using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Portfolio.Phone;

/// <summary>
/// The phone's surprises, drawn over whatever is on screen: a notification from the bulldog
/// shortly after the phone is picked up, and one incoming call from a recruiter bot.
/// </summary>
public class PhoneSurprises : Panel
{
    private static readonly string[] s_dogLines =
    {
        "Has been asleep for 6 hours.",
        "Snoring detected under the desk.",
        "Dreamed of sausages. Again.",
        "Moved 4 cm. Went back to sleep.",
    };

    private static readonly IBrush s_ink = new SolidColorBrush(Color.Parse("#1E1E2A"));
    private static readonly IBrush s_soft = new SolidColorBrush(Color.Parse("#5A5A6A"));

    private readonly List<IDisposable> _pending = new();
    private readonly Random _random = new();
    private bool _called;
    private DispatcherTimer? _beat;

    /// <summary>
    /// Called each time the phone is picked up.
    /// </summary>
    public void Start()
    {
        // Neither arrives on cue: the dog's note comes some seconds in, on most pick-ups, and the
        // call comes later still, so each visit plays out differently.
        if (_random.NextDouble() < 0.75)
            Later(Between(6, 22), () => Notify(DogIcon(), "Bulldog", s_dogLines[_random.Next(s_dogLines.Length)]));
        if (!_called)
            Later(Between(35, 80), Ring);
    }

    /// <summary>
    /// Called when the phone is put down: nothing fires while it lies on the desk.
    /// </summary>
    public void Stop()
    {
        foreach (var timer in _pending)
            timer.Dispose();
        _pending.Clear();
        _beat?.Stop();
        _beat = null;
        Children.Clear();
        Portfolio.Hosting.Host.PlaySound?.Invoke("ring-stop");
    }

    private double Between(double from, double to) => from + _random.NextDouble() * (to - from);

    private void Later(double seconds, Action action)
        => _pending.Add(DispatcherTimer.RunOnce(action, TimeSpan.FromSeconds(seconds)));

    // Notification

    private void Notify(Control icon, string title, string body)
    {
        Portfolio.Hosting.Host.PlaySound?.Invoke("notify");
        var slide = new TranslateTransform(0, -130)
        {
            Transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = TranslateTransform.YProperty,
                    Duration = TimeSpan.FromMilliseconds(380),
                    Easing = new CubicEaseOut(),
                },
            },
        };

        var text = new StackPanel
        {
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new TextBlock { Text = title, FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = s_ink },
                new TextBlock { Text = body, FontSize = 12, Foreground = s_soft, TextWrapping = TextWrapping.Wrap },
            },
        };

        var row = new DockPanel();
        DockPanel.SetDock(icon, Dock.Left);
        var when = new TextBlock { Text = "now", FontSize = 11, Foreground = s_soft, Margin = new Thickness(8, 2, 0, 0) };
        DockPanel.SetDock(when, Dock.Right);
        row.Children.Add(icon);
        row.Children.Add(when);
        row.Children.Add(text);

        var banner = new Border
        {
            Child = row,
            Margin = new Thickness(10, 34, 10, 0),
            Padding = new Thickness(12, 10),
            CornerRadius = new CornerRadius(18),
            Background = new SolidColorBrush(Color.FromArgb(242, 250, 250, 252)),
            VerticalAlignment = VerticalAlignment.Top,
            RenderTransform = slide,
            Cursor = new Cursor(StandardCursorType.Hand),
        };

        void Dismiss()
        {
            slide.Y = -130;
            Later(0.4, () => Children.Remove(banner));
        }

        banner.Tapped += (_, _) => Dismiss();
        Children.Add(banner);

        // Set after it is in the tree, so the change animates.
        Dispatcher.UIThread.Post(() => slide.Y = 0, DispatcherPriority.Background);
        Later(6, Dismiss);
    }

    /// <summary>
    /// A sleeping bulldog's face, in a few rectangles.
    /// </summary>
    private static Control DogIcon()
    {
        var face = new Canvas { Width = 38, Height = 38 };
        void Part(double x, double y, double w, double h, string color)
        {
            var part = new Border { Width = w, Height = h, Background = new SolidColorBrush(Color.Parse(color)) };
            Canvas.SetLeft(part, x);
            Canvas.SetTop(part, y);
            face.Children.Add(part);
        }

        Part(0, 0, 9, 10, "#8A5A36");    // ears
        Part(29, 0, 9, 10, "#8A5A36");
        Part(9, 20, 20, 14, "#F4EFE6");  // muzzle
        Part(15, 19, 8, 5, "#2A1C16");   // nose
        Part(8, 13, 8, 2, "#2A1C16");    // closed eyes
        Part(22, 13, 8, 2, "#2A1C16");

        return new Border
        {
            Width = 38,
            Height = 38,
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(Color.Parse("#C98D5A")),
            ClipToBounds = true,
            Child = face,
        };
    }

    private static Control Initials(string text, string color, double size) => new Border
    {
        Width = size,
        Height = size,
        CornerRadius = new CornerRadius(size / 2),
        Background = new SolidColorBrush(Color.Parse(color)),
        HorizontalAlignment = HorizontalAlignment.Center,
        Child = new TextBlock
        {
            Text = text,
            FontSize = size * 0.36,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        },
    };

    // Incoming call

    private void Ring()
    {
        _called = true;
        Portfolio.Hosting.Host.PlaySound?.Invoke("ring-start");

        var avatar = Initials("RB", "#5B6CFF", 92);
        var pulse = new ScaleTransform(1, 1);
        avatar.RenderTransform = pulse;

        var status = new TextBlock
        {
            Text = "incoming call…",
            FontSize = 14,
            Foreground = Brushes.White,
            Opacity = 0.75,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        var caller = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(0, 70, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                avatar,
                new TextBlock
                {
                    Text = "Recruiter Bot 3000",
                    FontSize = 22,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 12, 0, 0),
                },
                status,
            },
        };

        var buttons = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            Margin = new Thickness(20, 0, 20, 64),
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        var screen = new Grid
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops = { new GradientStop(Color.Parse("#1B2140"), 0), new GradientStop(Color.Parse("#0A0C18"), 1) },
            },
            Children = { caller, buttons },
        };

        // The avatar throbs while the phone rings.
        var beat = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
        var t = 0.0;
        beat.Tick += (_, _) =>
        {
            t += 0.04;
            pulse.ScaleX = pulse.ScaleY = 1 + 0.05 * Math.Sin(t * 7);
        };
        beat.Start();
        _beat = beat;

        void HangUp(string? missed)
        {
            beat.Stop();
            Portfolio.Hosting.Host.PlaySound?.Invoke("ring-stop");
            Children.Remove(screen);
            if (missed is not null)
                Notify(Initials("RB", "#5B6CFF", 38), "Missed call", missed);
        }

        var decline = CallButton("Decline", "#E5484D", HandsetIcon, 135);
        decline.Tapped += (_, _) => HangUp("Recruiter Bot 3000 will try again. And again.");
        buttons.Children.Add(decline);

        var polite = CallButton("Decline politely", "#3DA83D", MessageIcon);
        Grid.SetColumn(polite, 1);
        polite.Tapped += (_, _) =>
        {
            buttons.IsVisible = false;
            Portfolio.Hosting.Host.PlaySound?.Invoke("ring-stop");
            status.Text = "Sent: \"Happily employed at Avalonia, but thank you.\"";
            status.TextWrapping = TextWrapping.Wrap;
            status.TextAlignment = TextAlignment.Center;
            status.Margin = new Thickness(24, 0);
            Later(2.8, () => HangUp(null));
        };
        buttons.Children.Add(polite);

        Children.Add(screen);

        // Nobody answers a recruiter bot forever.
        Later(22, () =>
        {
            if (Children.Contains(screen) && buttons.IsVisible)
                HangUp("Recruiter Bot 3000 left 3 voicemails.");
        });
    }

    private const string HandsetIcon = "M6.62,10.79C8.06,13.62 10.38,15.94 13.21,17.38L15.41,15.18C15.69,14.9 16.08,14.82 16.43,14.93C17.55,15.3 18.75,15.5 20,15.5A1,1 0 0,1 21,16.5V20A1,1 0 0,1 20,21A17,17 0 0,1 3,4A1,1 0 0,1 4,3H7.5A1,1 0 0,1 8.5,4C8.5,5.25 8.7,6.45 9.07,7.57C9.18,7.92 9.1,8.31 8.82,8.59L6.62,10.79Z";
    private const string MessageIcon = "M20,2H4A2,2 0 0,0 2,4V22L6,18H20A2,2 0 0,0 22,16V4A2,2 0 0,0 20,2Z";

    private static Control CallButton(string label, string color, string icon, double turn = 0) => new StackPanel
    {
        Spacing = 8,
        Background = Brushes.Transparent,
        Cursor = new Cursor(StandardCursorType.Hand),
        Children =
        {
            new Border
            {
                Width = 62,
                Height = 62,
                CornerRadius = new CornerRadius(31),
                Background = new SolidColorBrush(Color.Parse(color)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new Avalonia.Controls.Shapes.Path
                {
                    Data = Geometry.Parse(icon),
                    Fill = Brushes.White,
                    Width = 26,
                    Height = 26,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    RenderTransform = new RotateTransform(turn),
                },
            },
            new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        },
    };
}
