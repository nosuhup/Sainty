using System.Diagnostics;
using Sainty.Models;

namespace Sainty.Services;

/// <summary>
/// Local process-name discovery. This does not inspect game memory or read account/session data.
/// </summary>
public sealed class GameDetector
{
    private static readonly (string DisplayName, string[] ProcessNames)[] KnownGames =
    [
        ("Roblox", ["RobloxPlayerBeta"]),
        ("Minecraft", ["javaw"]),
        ("Counter-Strike 2", ["cs2", "csgo"]),
        ("VALORANT", ["VALORANT-Win64-Shipping"]),
        ("Apex Legends", ["r5apex"]),
        ("Fortnite", ["FortniteClient-Win64-Shipping"]),
        ("League of Legends", ["League of Legends"]),
        ("Overwatch", ["Overwatch"])
    ];

    public IReadOnlyList<GameInstance> GetOpenInstances()
    {
        var found = new List<GameInstance>();

        foreach (var (displayName, processNames) in KnownGames)
        {
            foreach (var processName in processNames)
            {
                Process[] processes;

                try
                {
                    processes = Process.GetProcessesByName(processName);
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    continue;
                }

                foreach (var process in processes)
                {
                    using (process)
                    {
                        found.Add(new GameInstance(displayName, process.Id, DateTime.Now));
                    }
                }
            }
        }

        return found
            .OrderBy(game => game.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(game => game.ProcessId)
            .ToArray();
    }

    public GameInstance? DetectGame() => GetOpenInstances().FirstOrDefault();

    public static string GetFriendlyName(string processName)
    {
        foreach (var (displayName, processNames) in KnownGames)
        {
            if (processNames.Contains(processName, StringComparer.OrdinalIgnoreCase))
            {
                return displayName;
            }
        }

        return processName;
    }
}
