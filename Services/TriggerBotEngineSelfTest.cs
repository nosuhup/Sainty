using Sainty.Models;

namespace Sainty.Services;

/// <summary>Deterministic tests for trigger matching without desktop capture or input synthesis.</summary>
public static class TriggerBotEngineSelfTest
{
    public static async Task RunAsync()
    {
        var settings = new AppSettings
        {
            TBTargetColorHex = "#FF0000",
            TBTolerance = 0,
            TBPressCooldown = 100,
            TBDuplicateIgnore = 50
        };
        var engine = new TriggerBotEngine();
        engine.Configure(settings, new ScanRegion(0, 0, 10, 10));
        var firstHit = new TaskCompletionSource<TriggerMatch>(TaskCreationOptions.RunContinuationsAsynchronously);
        var hits = 0;
        engine.MatchDetected += (_, match) =>
        {
            hits++;
            firstHit.TrySetResult(match);
        };

        engine.Start((_, _) => ValueTask.FromResult<IReadOnlyList<PixelSample>>([
            new PixelSample(2, 3, new RgbColor(255, 0, 0))
        ]));

        var completed = await Task.WhenAny(firstHit.Task, Task.Delay(1000));
        await engine.StopAsync();
        if (completed != firstHit.Task) throw new InvalidOperationException("Scanner did not emit the expected synthetic match.");
        if (hits != 1) throw new InvalidOperationException($"Expected one trigger match, got {hits}.");
        if (firstHit.Task.Result.X != 2 || firstHit.Task.Result.Y != 3 || firstHit.Task.Result.Distance != 0)
            throw new InvalidOperationException("Synthetic match payload was incorrect.");
    }
}
