namespace Sainty.Models;

/// <summary>
/// Local settings reconstructed from the original MacroV2 AppSettings type.
/// Values are configuration only until the corresponding engine is implemented.
/// </summary>
public sealed class AppSettings
{
    public bool RunOnStartup { get; set; }
    public bool MinimizeToTray { get; set; }
    public bool PlayBeep { get; set; }
    public bool Topmost { get; set; }
    public bool Randomize { get; set; }
    public string Theme { get; set; } = "Dark";

    public int Aps { get; set; } = 15;
    public string ActionMode { get; set; } = "Mouse";
    public bool HoldMode { get; set; } = true;
    public string LastProfileName { get; set; } = "Default";
    public string MacroKeyToSpam { get; set; } = "LeftMouse";
    public string TriggerKey { get; set; } = "F6";
    public string TriggerMouse { get; set; } = "None";

    public string TBTargetColorHex { get; set; } = "#FF4040";
    public string TBKeyToPress { get; set; } = "E";
    public string TBMouseToPress { get; set; } = "None";
    public string TBActivationKey { get; set; } = "F7";
    public string TBActivationMouse { get; set; } = "None";
    public bool TBHoldMode { get; set; } = true;
    public int TBTolerance { get; set; } = 12;
    public int TBReactionTime { get; set; }
    public int TBPressCooldown { get; set; } = 75;
    public int TBDuplicateIgnore { get; set; } = 50;
    public int TBScanRadius { get; set; } = 6;
}
