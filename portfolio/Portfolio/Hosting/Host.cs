using System;

namespace Portfolio.Hosting;

/// <summary>
/// Hooks into whatever is hosting the OS. The browser head wires these to the 3D room;
/// on desktop they stay null and the related UI hides itself.
/// </summary>
public static class Host
{
    /// <summary>
    /// Leave the computer and go back to the room.
    /// </summary>
    public static Action? StandUp { get; set; }

    /// <summary>
    /// The player finished the game; the room puts a trophy on the shelf.
    /// </summary>
    public static Action? Trophy { get; set; }

    /// <summary>
    /// Open a live app, by URL, on the phone's screen.
    /// </summary>
    public static Action<string>? OpenPhoneApp { get; set; }

    /// <summary>
    /// Plays a named sound effect; see sound.js for the names.
    /// </summary>
    public static Action<string>? PlaySound { get; set; }

    /// <summary>
    /// Whether sound is switched on.
    /// </summary>
    public static Func<bool>? SoundOn { get; set; }

    /// <summary>
    /// Switches sound on or off and returns the new state.
    /// </summary>
    public static Func<bool>? ToggleSound { get; set; }

    /// <summary>
    /// Reads a value kept between visits; empty when there is none.
    /// </summary>
    /// <summary>
    /// Whether the visitor is looking at the laptop. False while they are in the room or on the
    /// phone, when nothing on the desktop should keep running.
    /// </summary>
    public static bool AtLaptop { get; set; } = true;

    public static Func<string, string>? ReadSetting { get; set; }

    /// <summary>
    /// Keeps a value between visits.
    /// </summary>
    public static Action<string, string>? WriteSetting { get; set; }

    /// <summary>
    /// The OS is on screen and can take input.
    /// </summary>
    public static Action? Ready { get; set; }
}
