namespace Sainty.Models;

/// <summary>
/// A named snapshot of the macro and pixel-trigger settings.
/// Property names intentionally track the recovered MacroV2.MacroProfile model.
/// </summary>
public sealed class MacroProfile
{
    public string Name { get; set; } = "Default";
    public int Aps { get; set; } = 15;
    public string ActionMode { get; set; } = "Mouse";
    public bool HoldMode { get; set; } = true;
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

    public static MacroProfile FromSettings(AppSettings settings, string name)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new MacroProfile
        {
            Name = name,
            Aps = settings.Aps,
            ActionMode = settings.ActionMode,
            HoldMode = settings.HoldMode,
            MacroKeyToSpam = settings.MacroKeyToSpam,
            TriggerKey = settings.TriggerKey,
            TriggerMouse = settings.TriggerMouse,
            TBTargetColorHex = settings.TBTargetColorHex,
            TBKeyToPress = settings.TBKeyToPress,
            TBMouseToPress = settings.TBMouseToPress,
            TBActivationKey = settings.TBActivationKey,
            TBActivationMouse = settings.TBActivationMouse,
            TBHoldMode = settings.TBHoldMode,
            TBTolerance = settings.TBTolerance,
            TBReactionTime = settings.TBReactionTime,
            TBPressCooldown = settings.TBPressCooldown,
            TBDuplicateIgnore = settings.TBDuplicateIgnore,
            TBScanRadius = settings.TBScanRadius
        };
    }

    public void ApplyTo(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        settings.Aps = Aps;
        settings.ActionMode = ActionMode;
        settings.HoldMode = HoldMode;
        settings.MacroKeyToSpam = MacroKeyToSpam;
        settings.TriggerKey = TriggerKey;
        settings.TriggerMouse = TriggerMouse;
        settings.TBTargetColorHex = TBTargetColorHex;
        settings.TBKeyToPress = TBKeyToPress;
        settings.TBMouseToPress = TBMouseToPress;
        settings.TBActivationKey = TBActivationKey;
        settings.TBActivationMouse = TBActivationMouse;
        settings.TBHoldMode = TBHoldMode;
        settings.TBTolerance = TBTolerance;
        settings.TBReactionTime = TBReactionTime;
        settings.TBPressCooldown = TBPressCooldown;
        settings.TBDuplicateIgnore = TBDuplicateIgnore;
        settings.TBScanRadius = TBScanRadius;
        settings.LastProfileName = Name;
    }
}
