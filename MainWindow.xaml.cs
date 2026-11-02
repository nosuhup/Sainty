using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Sainty.Models;
using Sainty.Services;

namespace Sainty;

public partial class MainWindow : Window
{
    private readonly ConfigStore _configStore = new();
    private readonly GameDetector _gameDetector = new();
    private readonly ObservableCollection<MacroProfile> _profiles = new();
    private AppSettings _settings;

    public MainWindow()
    {
        InitializeComponent();

        _settings = _configStore.LoadSettings();

        foreach (var profile in _configStore.LoadProfiles())
        {
            _profiles.Add(profile);
        }

        if (_profiles.Count == 0)
        {
            _profiles.Add(MacroProfile.FromSettings(_settings, "Default"));
        }

        ProfilesComboBox.ItemsSource = _profiles;
        ApplySettingsToControls();
        ProfilesComboBox.SelectedIndex = 0;
        ShowPage("Overview");
        RefreshGames();
    }

    private void Navigate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string page })
        {
            ShowPage(page);
        }
    }

    private void OpenClicker_Click(object sender, RoutedEventArgs e) => ShowPage("Clicker");

    private void OpenTriggerBot_Click(object sender, RoutedEventArgs e) => ShowPage("Trigger Bot");

    private void ShowPage(string page)
    {
        OverviewPage.Visibility = page == "Overview" ? Visibility.Visible : Visibility.Collapsed;
        ClickerPage.Visibility = page == "Clicker" ? Visibility.Visible : Visibility.Collapsed;
        TriggerBotPage.Visibility = page == "Trigger Bot" ? Visibility.Visible : Visibility.Collapsed;
        GamesPage.Visibility = page == "Games" ? Visibility.Visible : Visibility.Collapsed;
        ProfilesPage.Visibility = page == "Profiles" ? Visibility.Visible : Visibility.Collapsed;
        SettingsPage.Visibility = page == "Settings" ? Visibility.Visible : Visibility.Collapsed;

        CurrentPageTitle.Text = page;
        CurrentPageDescription.Text = page switch
        {
            "Overview" => "A local workspace rebuilt from recovered application metadata.",
            "Clicker" => "Macro parameters and activation settings.",
            "Trigger Bot" => "Pixel target parameters recovered from the original assembly.",
            "Games" => "Local process discovery without account or session access.",
            "Profiles" => "Save and restore configuration presets.",
            "Settings" => "Local application preferences.",
            _ => "Sainty"
        };
    }

    private void SaveClickerSettings_Click(object sender, RoutedEventArgs e) => SaveSettings("Clicker settings saved.");

    private void SaveTriggerSettings_Click(object sender, RoutedEventArgs e) => SaveSettings("Trigger settings saved.");

    private void SavePreferences_Click(object sender, RoutedEventArgs e) => SaveSettings("Application preferences saved.");

    private void SaveSettings(string message)
    {
        ReadControlsIntoSettings();
        _configStore.SaveSettings(_settings);
        Topmost = _settings.Topmost;
        StatusText.Text = $"{message} · {_configStore.SettingsPath}";
    }

    private void ReadControlsIntoSettings()
    {
        _settings.Aps = ParseInt(ApsTextBox.Text, _settings.Aps, 1, 1500);
        _settings.ActionMode = SelectedValue(ActionModeComboBox, _settings.ActionMode);
        _settings.MacroKeyToSpam = MacroKeyTextBox.Text.Trim();
        _settings.TriggerKey = ActivationKeyTextBox.Text.Trim();
        _settings.HoldMode = HoldModeCheckBox.IsChecked == true;
        _settings.Randomize = RandomizeCheckBox.IsChecked == true;

        _settings.TBTargetColorHex = NormalizeColor(TargetColorTextBox.Text, _settings.TBTargetColorHex);
        _settings.TBTolerance = ParseInt(ToleranceTextBox.Text, _settings.TBTolerance, 0, 255);
        _settings.TBReactionTime = ParseInt(ReactionTimeTextBox.Text, _settings.TBReactionTime, 0, 5000);
        _settings.TBPressCooldown = ParseInt(PressCooldownTextBox.Text, _settings.TBPressCooldown, 0, 5000);
        _settings.TBDuplicateIgnore = ParseInt(DuplicateIgnoreTextBox.Text, _settings.TBDuplicateIgnore, 0, 5000);
        _settings.TBScanRadius = ParseInt(ScanRadiusTextBox.Text, _settings.TBScanRadius, 1, 250);
        _settings.TBActivationKey = TriggerActivationKeyTextBox.Text.Trim();
        _settings.TBKeyToPress = TriggerKeyTextBox.Text.Trim();
        _settings.TBMouseToPress = SelectedValue(TriggerMouseComboBox, _settings.TBMouseToPress);
        _settings.TBHoldMode = TriggerHoldModeCheckBox.IsChecked == true;

        _settings.RunOnStartup = RunOnStartupCheckBox.IsChecked == true;
        _settings.MinimizeToTray = MinimizeToTrayCheckBox.IsChecked == true;
        _settings.PlayBeep = PlayBeepCheckBox.IsChecked == true;
        _settings.Topmost = TopmostCheckBox.IsChecked == true;
        _settings.Theme = SelectedValue(ThemeComboBox, "Dark");
    }

    private void ApplySettingsToControls()
    {
        ApsTextBox.Text = _settings.Aps.ToString();
        SelectComboValue(ActionModeComboBox, _settings.ActionMode);
        MacroKeyTextBox.Text = _settings.MacroKeyToSpam;
        ActivationKeyTextBox.Text = _settings.TriggerKey;
        HoldModeCheckBox.IsChecked = _settings.HoldMode;
        RandomizeCheckBox.IsChecked = _settings.Randomize;

        TargetColorTextBox.Text = _settings.TBTargetColorHex;
        ToleranceTextBox.Text = _settings.TBTolerance.ToString();
        ReactionTimeTextBox.Text = _settings.TBReactionTime.ToString();
        PressCooldownTextBox.Text = _settings.TBPressCooldown.ToString();
        DuplicateIgnoreTextBox.Text = _settings.TBDuplicateIgnore.ToString();
        ScanRadiusTextBox.Text = _settings.TBScanRadius.ToString();
        TriggerActivationKeyTextBox.Text = _settings.TBActivationKey;
        TriggerKeyTextBox.Text = _settings.TBKeyToPress;
        SelectComboValue(TriggerMouseComboBox, _settings.TBMouseToPress);
        TriggerHoldModeCheckBox.IsChecked = _settings.TBHoldMode;

        RunOnStartupCheckBox.IsChecked = _settings.RunOnStartup;
        MinimizeToTrayCheckBox.IsChecked = _settings.MinimizeToTray;
        PlayBeepCheckBox.IsChecked = _settings.PlayBeep;
        TopmostCheckBox.IsChecked = _settings.Topmost;
        SelectComboValue(ThemeComboBox, _settings.Theme);
        Topmost = _settings.Topmost;
    }

    private static string SelectedValue(ComboBox comboBox, string fallback) =>
        comboBox.SelectedValue?.ToString() ?? fallback;

    private static void SelectComboValue(ComboBox comboBox, string value)
    {
        comboBox.SelectedValue = value;
        if (comboBox.SelectedIndex < 0 && comboBox.Items.Count > 0)
        {
            comboBox.SelectedIndex = 0;
        }
    }

    private static int ParseInt(string? value, int fallback, int minimum, int maximum)
    {
        if (!int.TryParse(value, out var parsed))
        {
            return fallback;
        }

        return Math.Clamp(parsed, minimum, maximum);
    }

    private static string NormalizeColor(string? value, string fallback)
    {
        var candidate = value?.Trim() ?? string.Empty;
        if (candidate.Length == 7 && candidate[0] == '#' &&
            candidate.AsSpan(1).ToString().All(Uri.IsHexDigit))
        {
            return candidate.ToUpperInvariant();
        }

        return fallback;
    }

    private void RefreshGames_Click(object sender, RoutedEventArgs e) => RefreshGames();

    private void RefreshGames()
    {
        var games = _gameDetector.GetOpenInstances();
        GamesListBox.ItemsSource = games;
        GamesCountText.Text = games.Count == 1 ? "1 process detected" : $"{games.Count} processes detected";
    }

    private void SaveProfile_Click(object sender, RoutedEventArgs e)
    {
        ReadControlsIntoSettings();

        var name = string.IsNullOrWhiteSpace(ProfileNameTextBox.Text)
            ? "Profile"
            : ProfileNameTextBox.Text.Trim();

        _settings.LastProfileName = name;
        var profile = MacroProfile.FromSettings(_settings, name);
        var existingIndex = -1;

        for (var i = 0; i < _profiles.Count; i++)
        {
            if (string.Equals(_profiles[i].Name, name, StringComparison.OrdinalIgnoreCase))
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex >= 0)
        {
            _profiles[existingIndex] = profile;
        }
        else
        {
            _profiles.Add(profile);
        }

        ProfilesComboBox.SelectedItem = profile;
        _configStore.SaveProfiles(_profiles);
        _configStore.SaveSettings(_settings);
        StatusText.Text = $"Profile '{name}' saved locally.";
    }

    private void ApplyProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesComboBox.SelectedItem is not MacroProfile profile)
        {
            StatusText.Text = "Select a profile first.";
            return;
        }

        profile.ApplyTo(_settings);
        ProfileNameTextBox.Text = profile.Name;
        ApplySettingsToControls();
        _configStore.SaveSettings(_settings);
        StatusText.Text = $"Profile '{profile.Name}' applied.";
    }

    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesComboBox.SelectedItem is not MacroProfile profile)
        {
            StatusText.Text = "Select a profile first.";
            return;
        }

        _profiles.Remove(profile);
        if (_profiles.Count == 0)
        {
            _profiles.Add(MacroProfile.FromSettings(_settings, "Default"));
        }

        ProfilesComboBox.SelectedIndex = 0;
        _configStore.SaveProfiles(_profiles);
        StatusText.Text = $"Profile '{profile.Name}' deleted.";
    }
}
