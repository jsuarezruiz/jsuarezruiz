using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Portfolio.Content;
using Portfolio.Pixel;

namespace Portfolio.Apps;

/// <summary>
/// A CRT-green terminal drawn entirely with the bitmap font.
/// </summary>
public class TerminalApp : Control
{
    private const double Px = 2;
    private const double LineHeight = (PixelFont.GlyphHeight + 3) * Px;
    private const double Pad = 12;

    private static readonly IBrush s_back = Palette.Brush(Palette.TerminalBack);
    private static readonly IBrush s_text = Palette.Brush(Palette.TerminalGreen);

    private readonly List<string> _lines = new();
    private string _input = "";

    public TerminalApp()
    {
        Width = 540;
        Height = 330;
        Focusable = true;
        ClipToBounds = true;

        Print("SUAREZ-OS TERMLINK");
        Print("TYPE HELP FOR COMMANDS");
        Print("");
    }

    /// <summary>
    /// Raised with an app id when a command asks the shell to open something.
    /// </summary>
    public event Action<string>? OpenRequested;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter)
        {
            Print("> " + _input);
            Run(_input.Trim().ToLowerInvariant());
            _input = "";
        }
        else if (e.Key == Key.Back)
        {
            if (_input.Length > 0)
                _input = _input[..^1];
        }
        else
        {
            return;
        }

        e.Handled = true;
        InvalidateVisual();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        if (string.IsNullOrEmpty(e.Text))
            return;

        foreach (var ch in e.Text)
        {
            if (!char.IsControl(ch) && _input.Length < 40)
                _input += ch;
        }

        e.Handled = true;
        InvalidateVisual();
    }

    private void Print(string line) => _lines.Add(line);

    private void Run(string command)
    {
        var content = PortfolioContent.Current;
        switch (command)
        {
            case "":
                break;
            case "help":
                Print("LS  WHOAMI  STATS  HUNT  OPEN PROJECTS");
                Print("OPEN RESUME  MAIL  ASK  SCREENSAVER");
                Print("SUDO HIRE  CLEAR");
                break;
            case "ls":
                foreach (var era in content.Eras)
                    Print($"{era.Id}/  {content.ByEra(era.Id).Count()} repos  {era.Years}");
                break;
            case "whoami":
                Print(content.Profile.Name);
                Print("Software Engineer at Avalonia");
                break;
            case "stats":
                Print($"{content.Profile.Repos} repos");
                Print($"{content.Profile.Stars} stars");
                Print($"{content.Profile.Followers} followers");
                break;
            case "hunt":
                OpenRequested?.Invoke("game");
                break;
            case "open projects":
                OpenRequested?.Invoke("projects");
                break;
            case "open resume":
                OpenRequested?.Invoke("resume");
                break;
            case "mail":
                OpenRequested?.Invoke("mail");
                break;
            case "ask":
                OpenRequested?.Invoke("chat");
                break;
            case "crash":
            case "rm -rf /":
            case "format c:":
                OpenRequested?.Invoke("crash");
                break;
            case "screensaver":
                OpenRequested?.Invoke("screensaver");
                break;
            case "sudo hire":
                Portfolio.Shell.Achievements.Unlock("nice-try");
                Print("PERMISSION DENIED: HAPPILY EMPLOYED");
                break;
            case "clear":
                _lines.Clear();
                break;
            default:
                Print("UNKNOWN COMMAND. TRY HELP");
                break;
        }
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(s_back, new Rect(Bounds.Size));

        var visible = Math.Max(1, (int)((Bounds.Height - Pad * 2) / LineHeight) - 1);
        var first = Math.Max(0, _lines.Count - visible);
        var y = Pad;
        for (var i = first; i < _lines.Count; i++)
        {
            PixelFont.Draw(context, _lines[i], Pad, y, Px, s_text);
            y += LineHeight;
        }

        PixelFont.Draw(context, "> " + _input + "_", Pad, y, Px, s_text);
    }
}
