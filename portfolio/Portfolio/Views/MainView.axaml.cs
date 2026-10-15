using Avalonia.Controls;
using Avalonia.Interactivity;
using Portfolio.Hosting;
using Portfolio.Phone;
using Portfolio.Shell;

namespace Portfolio.Views;

public partial class MainView : UserControl
{
    private readonly DesktopShell _desktop = new();
    private PhoneShell? _phone;

    public MainView()
    {
        InitializeComponent();
        Current = this;
        Content = _desktop;
    }

    /// <summary>
    /// The live view, so the host can switch between the laptop and the phone.
    /// </summary>
    public static MainView? Current { get; private set; }

    /// <summary>
    /// Shows the laptop's OS ("desktop") or the phone's home screen ("phone"). "room" means the
    /// visitor walked away from both: the phone is put down and the desktop goes quiet.
    /// </summary>
    public void SetMode(string mode)
    {
        Host.AtLaptop = mode == "desktop";
        Content = mode == "phone" ? _phone ??= new PhoneShell() : _desktop;
        if (mode == "room")
            _desktop.Leave();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Host.Ready?.Invoke();
    }
}
