using Sainty.Models;

namespace Sainty.Services;

/// <summary>Deterministic tests for trigger matching without desktop capture or input synthesis.</summary>
public static class TriggerBotEngineSelfTest
{
    public static async Task RunAsync()
    {
        var settings = new AppSettings { TBTargetColorHex = "#FF0000", TBTolerance = 0, TBPressCooldown = 100, TBDuplicateIgnore = 50 };
        var engine = new TriggerBotEngine();
        engine.Configure(settings, new ScanRegion(0, 0, 10, 10));
        var hits = 0;
        engine.MatchDetected += (_, _) => hits++;
        engine.Start((_, _) => ValueTask.FromResult<IReadOnlyList<PixelSample>>([
            new PixelSample(2, 3, new RgbColor(255, 0, 0))
        ]));
        await Task.Delay(20);
        await engine.StopAsync();
        if (hits != 1) throw new InvalidOperationException($"Expected one trigger match, got {hits}.");
    }
}
