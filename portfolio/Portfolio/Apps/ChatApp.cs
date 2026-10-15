using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Portfolio.Pixel;

namespace Portfolio.Apps;

/// <summary>
/// AskGPT: a chat window that answers questions about the portfolio's owner. It looks like
/// an AI assistant and says plainly that it is not one; the answers come from <see cref="ChatBrain"/>.
/// </summary>
public class ChatApp : DockPanel
{
    private const int FreeMessages = 10;

    private static readonly Color s_user = Palette.TitleBlue;
    private static readonly Color s_bot = Color.Parse("#FFFFFF");

    private readonly ChatBrain _brain = new();
    private readonly StackPanel _messages = new() { Spacing = 9, Margin = new Thickness(12) };
    private readonly ScrollViewer _scroll;
    private readonly TextBox _input;
    private readonly DispatcherTimer _typing = new() { Interval = TimeSpan.FromMilliseconds(38) };
    private string[] _words = Array.Empty<string>();
    private TextBlock? _target;
    private ChatReply? _pending;
    private StackPanel? _pendingBubble;
    private int _word;
    private int _sent;

    public ChatApp()
    {
        Width = 560;
        Height = 460;

        var header = new DockPanel { Margin = new Thickness(12, 9) };
        var model = new PixelText { Text = "MODEL: IF-ELSE-4O-MINI", Foreground = Palette.Brush(Palette.InkSoft), VerticalAlignment = VerticalAlignment.Center };
        SetDock(model, Dock.Right);
        header.Children.Add(model);
        header.Children.Add(new PixelText { Text = "ASKGPT", PixelSize = 3, Foreground = Palette.Brush(Palette.Ink) });
        SetDock(header, Dock.Top);
        Children.Add(header);

        _input = new TextBox
        {
            PlaceholderText = "Ask about Javier",
            FontSize = 13,
            CornerRadius = new CornerRadius(0),
            BorderThickness = new Thickness(3),
            BorderBrush = Palette.Brush(Palette.Bevel),
            Background = Brushes.White,
            Foreground = Palette.Brush(Palette.Ink),
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        _input.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter)
                return;
            e.Handled = true;
            Send(_input.Text);
        };

        var send = new PixelButton("SEND", Palette.StartGreen) { Margin = new Thickness(6, 0, 0, 0) };
        send.Click += (_, _) => Send(_input.Text);

        var entry = new DockPanel { Margin = new Thickness(12, 0, 12, 12) };
        SetDock(send, Dock.Right);
        entry.Children.Add(send);
        entry.Children.Add(_input);
        SetDock(entry, Dock.Bottom);
        Children.Add(entry);

        var chips = new WrapPanel { Margin = new Thickness(12, 6, 12, 6) };
        foreach (var question in ChatBrain.Suggestions)
        {
            var chip = new Border
            {
                Margin = new Thickness(0, 0, 6, 6),
                Padding = new Thickness(9, 4),
                Background = Palette.Brush(Palette.Paper),
                BorderBrush = Palette.Brush(Palette.Bevel),
                BorderThickness = new Thickness(2),
                Cursor = new Cursor(StandardCursorType.Hand),
                Child = new TextBlock { Text = question, FontSize = 12, Foreground = Palette.Brush(Palette.Ink) },
            };
            chip.Tapped += (_, _) => Send(question);
            chips.Children.Add(chip);
        }
        SetDock(chips, Dock.Bottom);
        Children.Add(chips);

        _scroll = new ScrollViewer { Content = _messages };
        Children.Add(new Border
        {
            Background = Palette.Brush(Color.Parse("#E9E6DA")),
            BorderBrush = Palette.Brush(Palette.Bevel),
            BorderThickness = new Thickness(0, 3, 0, 3),
            Child = _scroll,
        });

        _typing.Tick += (_, _) => TypeNextWord();

        Say(new ChatReply("Hi. I'm AskGPT. I know one subject very well: the person whose desk this is. I am not a real AI, so keep the questions short."));
    }

    /// <summary>
    /// The visitor pressed a button in an answer that opens another app.
    /// </summary>
    public event Action<string>? OpenRequested;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _input.Focus();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _typing.Stop();
    }

    private void Send(string? text)
    {
        // One answer at a time: wait for the current one to finish typing.
        if (string.IsNullOrWhiteSpace(text) || _typing.IsEnabled)
            return;

        _input.Text = "";
        Bubble(text.Trim(), fromUser: true);
        _sent++;
        Portfolio.Shell.Achievements.Unlock("prompt");

        var reply = _brain.Answer(text);
        if (_sent == FreeMessages)
            reply = reply with { Text = reply.Text + " (You've also reached the free limit. Upgrade by starring a repo. I'll keep answering anyway.)" };
        Say(reply);
    }

    /// <summary>
    /// Shows an answer, typed out a word at a time after a short "thinking" pause.
    /// </summary>
    private void Say(ChatReply reply)
    {
        var (bubble, text) = Bubble("...", fromUser: false);
        _pending = reply;
        _pendingBubble = bubble;
        _target = text;
        _words = reply.Text.Split(' ');
        _word = -12; // ticks spent "thinking" before the first word
        _typing.Start();
    }

    private void TypeNextWord()
    {
        if (_target is null)
            return;

        if (_word < 0)
        {
            _word++;
            return;
        }

        if (_word < _words.Length)
        {
            _target.Text = _word == 0 ? _words[0] : _target.Text + " " + _words[_word];
            _word++;
            _scroll.ScrollToEnd();
            return;
        }

        _typing.Stop();
        if (_pending is { ActionLabel: { } label } reply && _pendingBubble is { } bubble)
        {
            var action = new PixelButton(label, Palette.TitleBlue) { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 6, 0, 0) };
            action.Click += (_, _) => Act(reply);
            bubble.Children.Add(action);
            _scroll.ScrollToEnd();
        }
        _input.Focus();
    }

    private async void Act(ChatReply reply)
    {
        if (reply.AppId is { } app)
            OpenRequested?.Invoke(app);
        else if (reply.Url is { } url && TopLevel.GetTopLevel(this) is { } top)
            await top.Launcher.LaunchUriAsync(new Uri(url));
    }

    private (StackPanel Bubble, TextBlock Text) Bubble(string text, bool fromUser)
    {
        var body = new TextBlock
        {
            Text = text,
            FontSize = 13,
            LineHeight = 19,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Palette.Brush(fromUser ? Palette.White : Palette.Ink),
        };
        var content = new StackPanel { Children = { body } };

        _messages.Children.Add(new Border
        {
            Child = content,
            MaxWidth = 380,
            Padding = new Thickness(12, 8),
            HorizontalAlignment = fromUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Background = Palette.Brush(fromUser ? s_user : s_bot),
            BorderBrush = Palette.Brush(fromUser ? Palette.BorderBlue : Palette.Bevel),
            BorderThickness = new Thickness(0, 0, 3, 3),
        });
        _scroll.ScrollToEnd();
        return (content, body);
    }
}
