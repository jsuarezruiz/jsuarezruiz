using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Portfolio;
using Portfolio.Browser;
using Portfolio.Hosting;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        Host.StandUp = Interop.StandUp;
        Host.Trophy = Interop.Trophy;
        Host.Ready = Interop.Ready;
        Host.OpenPhoneApp = Interop.OpenPhoneApp;
        Host.PlaySound = Interop.PlaySound;
        Host.SoundOn = Interop.SoundOn;
        Host.ToggleSound = Interop.ToggleSound;
        Host.ReadSetting = Interop.ReadSetting;
        Host.WriteSetting = Interop.WriteSetting;

        return BuildAvaloniaApp()
            .WithInterFont()
#if DEBUG
            .WithDeveloperTools()
#endif
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
