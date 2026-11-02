using System.Text.Json;
using Sainty.Models;

namespace Sainty.Services;

/// <summary>
/// Reads and writes Sainty's local configuration under the current user's profile.
/// Writes use a temporary file so an interrupted save does not truncate the last good file.
/// </summary>
public sealed class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _directory;

    public ConfigStore()
    {
        _directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Sainty");
    }

    public string SettingsPath => Path.Combine(_directory, "settings.json");

    public string ProfilesPath => Path.Combine(_directory, "profiles.json");

    public AppSettings LoadSettings() => ReadJson<AppSettings>(SettingsPath) ?? new AppSettings();

    public List<MacroProfile> LoadProfiles() =>
        ReadJson<List<MacroProfile>>(ProfilesPath) ?? new List<MacroProfile>();

    public void SaveSettings(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        WriteJson(SettingsPath, settings);
    }

    public void SaveProfiles(IEnumerable<MacroProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        WriteJson(ProfilesPath, profiles.ToList());
    }

    private T? ReadJson<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
        catch (IOException)
        {
            return default;
        }
        catch (UnauthorizedAccessException)
        {
            return default;
        }
    }

    private void WriteJson<T>(string path, T value)
    {
        Directory.CreateDirectory(_directory);

        var temporaryPath = path + ".tmp";
        var json = JsonSerializer.Serialize(value, JsonOptions);
        File.WriteAllText(temporaryPath, json);

        File.Move(temporaryPath, path, overwrite: true);
    }
}
