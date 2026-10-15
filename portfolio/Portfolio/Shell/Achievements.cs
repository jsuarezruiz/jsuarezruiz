using System;
using System.Collections.Generic;
using System.Linq;
using Portfolio.Hosting;

namespace Portfolio.Shell;

public sealed record Achievement(string Id, string Title, string How);

/// <summary>
/// Things a visitor can do on the desktop, and which of them they have done. Any part of the
/// OS calls <see cref="Unlock"/>; the shell shows a toast, and the list is kept between visits.
/// </summary>
public static class Achievements
{
    private const string Key = "achievements";

    public static readonly IReadOnlyList<Achievement> All = new Achievement[]
    {
        new("first-app", "HELLO WORLD", "Open your first app."),
        new("explorer", "EXPLORER", "Open every app on the desktop."),
        new("due-diligence", "DUE DILIGENCE", "Read the resume."),
        new("prompt", "PROMPT ENGINEER", "Ask AskGPT a question."),
        new("pen-pal", "PEN PAL", "Send a message from Mail."),
        new("bug-catcher", "BUG CATCHER", "Catch a bug in Bug Hunt."),
        new("shipped", "SHIPPED IT", "Reach the flag in Bug Hunt."),
        new("zero-bugs", "ZERO KNOWN BUGS", "Ship with every bug caught."),
        new("loop-breaker", "LOOP BREAKER", "Beat the Layout Cycle in Bug Hunt."),
        new("survivor", "SURVIVOR", "Come back from a blue screen."),
        new("nice-try", "NICE TRY", "Run sudo hire in the Terminal."),
        new("compiles", "IT COMPILES", "Build a file in Code Studio without errors."),
        new("designer", "INTERIOR DESIGNER", "Move an icon."),
        new("decorator", "DECORATOR", "Change the wallpaper."),
        new("refresher", "F5 ENJOYER", "Refresh the desktop. Nothing happens."),
        new("hoarder", "HOARDER", "Try to empty the Recycle Bin."),
        new("afk", "AWAY FROM KEYBOARD", "Let the screensaver start."),
    };

    private static HashSet<string>? s_unlocked;

    /// <summary>
    /// While true nothing unlocks, so a test run leaves no marks.
    /// </summary>
    public static bool Muted { get; set; }

    /// <summary>
    /// Raised once, the first time an achievement is earned.
    /// </summary>
    public static event Action<Achievement>? Unlocked;

    private static HashSet<string> Earned => s_unlocked ??= Load();

    public static int Count => Earned.Count;

    public static bool Has(string id) => Earned.Contains(id);

    public static void Unlock(string id)
    {
        var achievement = All.FirstOrDefault(a => a.Id == id);
        if (Muted || achievement is null || !Earned.Add(id))
            return;

        Host.WriteSetting?.Invoke(Key, string.Join(',', Earned));
        Unlocked?.Invoke(achievement);
    }

    private static HashSet<string> Load()
    {
        var saved = Host.ReadSetting?.Invoke(Key) ?? "";
        var known = All.Select(a => a.Id).ToHashSet();
        return saved.Split(',', StringSplitOptions.RemoveEmptyEntries).Where(known.Contains).ToHashSet();
    }
}
