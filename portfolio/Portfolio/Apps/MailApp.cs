using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Portfolio.Content;
using Portfolio.Pixel;

namespace Portfolio.Apps;

/// <summary>
/// A small mail client. The inbox is for fun; composing is real: Send hands the message
/// to the visitor's own mail app, addressed and filled in.
/// </summary>
public class MailApp : DockPanel
{
    private sealed record Message(string From, string Subject, string Body);

    private static readonly Message[] s_inbox =
    {
        new("Javier", "Welcome to my inbox",
            "Thanks for stopping by.\n\nPress NEW MESSAGE to write to me. It opens your own mail app with everything " +
            "filled in, so nothing is sent from this page.\n\nI read everything."),
        new("NullReferenceException", "Object reference not set",
            "You caught me once in Bug Hunt.\n\nI will be back. I always come back."),
        new("Recruiter Bot 3000", "Exciting opportunity!!!",
            "Hi {first_name},\n\nI came across your profile and was impressed by your {skill}.\n\n" +
            "Reply from Javier: happily employed at Avalonia, but thank you."),
        new("Javier (2014)", "Is this XAML thing going anywhere?",
            "Just started with Xamarin.Forms. Not sure cross-platform UI has a future.\n\n" +
            "Reply from Javier: keep going."),
    };

    private readonly ContentControl _body = new();
    private readonly string _address;
    private readonly string _owner;

    public MailApp()
    {
        Width = 600;
        Height = 400;

        var content = PortfolioContent.Current;
        _owner = content.Profile.Name;
        _address = content.Links.Select(l => l.Url).FirstOrDefault(u => u.StartsWith("mailto:", StringComparison.Ordinal))?[7..] ?? "";

        var compose = new PixelButton("NEW MESSAGE", Palette.StartGreen);
        compose.Click += (_, _) => ShowCompose("");
        var inbox = new PixelButton("INBOX", Palette.PaperDark, Palette.Ink);
        inbox.Click += (_, _) => ShowInbox(0);

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(9, 9, 9, 6),
            Children = { compose, inbox },
        };
        SetDock(toolbar, Dock.Top);
        Children.Add(toolbar);
        Children.Add(_body);

        ShowInbox(0);
    }

    private void ShowInbox(int selected)
    {
        var list = new StackPanel { Width = 210 };
        for (var i = 0; i < s_inbox.Length; i++)
        {
            var index = i;
            var message = s_inbox[i];
            var isSelected = i == selected;
            var row = new Border
            {
                Padding = new Thickness(9, 6),
                Background = isSelected ? Palette.Brush(Palette.TitleBlue) : Brushes.Transparent,
                BorderBrush = Palette.Brush(Palette.PaperDark),
                BorderThickness = new Thickness(0, 0, 0, 3),
                Cursor = new Cursor(StandardCursorType.Hand),
                Child = new StackPanel
                {
                    Spacing = 2,
                    Children =
                    {
                        Text(message.From, 13, FontWeight.SemiBold, isSelected ? Palette.White : Palette.Ink),
                        Text(message.Subject, 12, FontWeight.Normal, isSelected ? Palette.White : Palette.InkSoft, wrap: false),
                    },
                },
            };
            row.Tapped += (_, _) => ShowInbox(index);
            list.Children.Add(row);
        }

        var open = s_inbox[selected];
        var reply = new PixelButton("REPLY", Palette.TitleBlue) { HorizontalAlignment = HorizontalAlignment.Left };
        reply.Click += (_, _) => ShowCompose("Re: " + open.Subject);

        var reading = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(15, 12),
            Children =
            {
                Text(open.Subject, 16, FontWeight.SemiBold, Palette.Ink),
                Text("From: " + open.From, 12, FontWeight.Normal, Palette.InkSoft),
                new Border { Height = 3, Background = Palette.Brush(Palette.PaperDark), Margin = new Thickness(0, 3) },
                Text(open.Body, 13, FontWeight.Normal, Palette.Ink),
                new Border { Height = 6 },
                reply,
            },
        };

        var layout = new DockPanel();
        var left = new Border
        {
            Child = new ScrollViewer { Content = list },
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(0, 3, 3, 0),
        };
        SetDock(left, Dock.Left);
        layout.Children.Add(left);
        layout.Children.Add(new Border
        {
            Child = new ScrollViewer { Content = reading },
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(0, 3, 0, 0),
        });
        _body.Content = layout;
    }

    private void ShowCompose(string subject)
    {
        var subjectBox = Field(subject, "Subject");
        var messageBox = Field("", "Write your message");
        messageBox.AcceptsReturn = true;
        messageBox.TextWrapping = TextWrapping.Wrap;
        messageBox.VerticalContentAlignment = VerticalAlignment.Top;

        var status = new PixelText { Foreground = Palette.Brush(Palette.StartGreenDark), VerticalAlignment = VerticalAlignment.Center };

        var send = new PixelButton("SEND", Palette.StartGreen);
        send.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(messageBox.Text))
            {
                status.Foreground = Palette.Brush(Palette.CloseRed);
                status.Text = "WRITE A MESSAGE FIRST";
                return;
            }

            var uri = $"mailto:{_address}?subject={Uri.EscapeDataString(subjectBox.Text ?? "")}&body={Uri.EscapeDataString(messageBox.Text)}";
            if (TopLevel.GetTopLevel(this) is { } top)
                await top.Launcher.LaunchUriAsync(new Uri(uri));

            status.Foreground = Palette.Brush(Palette.StartGreenDark);
            status.Text = "OPENED IN YOUR MAIL APP";
            Portfolio.Shell.Achievements.Unlock("pen-pal");
        };

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { send, status } };
        SetDock(actions, Dock.Bottom);

        var header = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(0, 0, 0, 6),
            Children =
            {
                Text("To: " + _owner, 13, FontWeight.SemiBold, Palette.Ink),
                subjectBox,
            },
        };
        SetDock(header, Dock.Top);

        var form = new DockPanel { Margin = new Thickness(12, 9, 12, 12) };
        actions.Margin = new Thickness(0, 9, 0, 0);
        form.Children.Add(header);
        form.Children.Add(actions);
        form.Children.Add(messageBox);

        _body.Content = new Border
        {
            Child = form,
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(0, 3, 0, 0),
        };
        (string.IsNullOrEmpty(subject) ? subjectBox : messageBox).Focus();
    }

    private static TextBox Field(string text, string placeholder) => new()
    {
        Text = text,
        PlaceholderText = placeholder,
        FontSize = 13,
        CornerRadius = new CornerRadius(0),
        BorderThickness = new Thickness(3),
        BorderBrush = Palette.Brush(Palette.Bevel),
        Background = Palette.Brush(Palette.White),
        Foreground = Palette.Brush(Palette.Ink),
    };

    private static TextBlock Text(string text, double size, FontWeight weight, Color color, bool wrap = true) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = weight,
        Foreground = Palette.Brush(color),
        TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
        TextTrimming = TextTrimming.CharacterEllipsis,
    };
}
