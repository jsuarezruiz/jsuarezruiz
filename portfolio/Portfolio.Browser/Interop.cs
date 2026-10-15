using System.Runtime.InteropServices.JavaScript;
using Avalonia.Threading;
using Portfolio.Shell;
using Portfolio.Views;

namespace Portfolio.Browser;

/// <summary>
/// The bridge between the OS and the 3D room in the page.
/// </summary>
internal static partial class Interop
{
    [JSImport("standUp", "main.js")]
    internal static partial void StandUp();

    [JSImport("trophy", "main.js")]
    internal static partial void Trophy();

    [JSImport("openPhoneApp", "main.js")]
    internal static partial void OpenPhoneApp(string url);

    [JSImport("playSound", "main.js")]
    internal static partial void PlaySound(string name);

    [JSImport("soundOn", "main.js")]
    internal static partial bool SoundOn();

    [JSImport("toggleSound", "main.js")]
    internal static partial bool ToggleSound();

    [JSImport("readSetting", "main.js")]
    internal static partial string ReadSetting(string key);

    [JSImport("writeSetting", "main.js")]
    internal static partial void WriteSetting(string key, string value);

    [JSImport("ready", "main.js")]
    internal static partial void Ready();

    [JSExport]
    internal static void SetMode(string mode)
        => Dispatcher.UIThread.Post(() => MainView.Current?.SetMode(mode));

#if DEBUG
    /// <summary>
    /// Runs the boss fight with a bot and returns its report. For testing from the browser console.
    /// </summary>
    [JSExport]
    internal static string TestBoss() => Portfolio.Game.GameView.RunBossTest();
#endif

    [JSExport]
    internal static void Boot()
        => Dispatcher.UIThread.Post(() => DesktopShell.Current?.Boot());

    [JSExport]
    internal static void OpenApp(string id)
        => Dispatcher.UIThread.Post(() => DesktopShell.Current?.OpenApp(id));
}
