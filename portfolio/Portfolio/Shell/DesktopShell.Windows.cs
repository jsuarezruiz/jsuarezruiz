using System;
using System.Linq;
using Avalonia.Controls;
using Portfolio.Apps;
using Portfolio.Game;
using Portfolio.Hosting;

namespace Portfolio.Shell;

public partial class DesktopShell
{
    public void OpenApp(string id)
    {
        _balloon.IsVisible = false;
        _startMenu.IsVisible = false;

        if (id == "crash")
        {
            Crash();
            return;
        }

        if (id == "screensaver")
        {
            StartScreensaver();
            return;
        }

        if (_windows.Children.OfType<PixelWindow>().FirstOrDefault(w => w.Id == id) is { } existing)
        {
            if (existing.IsMinimised)
                existing.Restore();
            Activate(existing);
            return;
        }

        var (title, content) = Create(id);
        if (content is null)
            return;

        Achievements.Unlock("first-app");
        if (id == "resume")
            Achievements.Unlock("due-diligence");
        _opened.Add(id);
        if (s_appIds.All(_opened.Contains))
            Achievements.Unlock("explorer");

        var window = new PixelWindow(id, title, content);
        window.Closed += (_, _) =>
        {
            _windows.Children.Remove(window);
            if (_activeWindow == window)
                _activeWindow = null;
            Host.PlaySound?.Invoke("close");
            RefreshTasks();
        };
        window.Activated += (_, _) => Activate(window);
        window.StateChanged += (_, _) =>
        {
            if (window.IsMinimised && _activeWindow == window)
                _activeWindow = null;
            RefreshTasks();
        };
        Host.PlaySound?.Invoke("open");

        var offset = 27 * (_cascade++ % 5);
        Canvas.SetLeft(window, 198 + offset);
        Canvas.SetTop(window, 24 + offset);
        _windows.Children.Add(window);
        Activate(window);
    }

    private void Activate(PixelWindow window)
    {
        window.ZIndex = ++_z;
        _activeWindow = window;
        RefreshTasks();
    }

    private async void OpenLink(string url)
    {
        _startMenu.IsVisible = false;
        if (TopLevel.GetTopLevel(this) is { } top)
            await top.Launcher.LaunchUriAsync(new Uri(url));
    }

    private (string Title, Control? Content) Create(string id)
    {
        switch (id)
        {
            case "projects":
                return ("MY PROJECTS", new ProjectsApp());
            case "resume":
                var resume = new ResumeApp();
                resume.EmailRequested += (_, _) => OpenApp("mail");
                return ("RESUME.DOC", resume);
            case "terminal":
                var terminal = new TerminalApp();
                terminal.OpenRequested += OpenApp;
                return ("TERMINAL", terminal);
            case "mail":
                return ("MAIL", new MailApp());
            case "browser":
                return ("INTERNET", new BrowserApp());
            case "chat":
                var chat = new ChatApp();
                chat.OpenRequested += OpenApp;
                return ("ASKGPT", chat);
            case "photos":
                return ("PHOTOS", new PhotosApp());
            case "studio":
                return ("CODE STUDIO", new StudioApp());
            case "achievements":
                return ("ACHIEVEMENTS", new AchievementsApp());
            case "bin":
                return ("RECYCLE BIN", new RecycleBinApp());
            case "game":
                // Integer scale only, so the pixel art stays crisp.
                var roomy = Bounds.Width >= 900 && Bounds.Height >= 560;
                var game = new GameView(roomy ? 3 : 2);
                game.ProjectsRequested += (_, _) => OpenApp("projects");
                return ("BUGHUNT.EXE", game);
            default:
                return ("", null);
        }
    }
}
