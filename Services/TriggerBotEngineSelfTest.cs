using Sainty.Models;

namespace Sainty.Services;

/// <summary>Deterministic trigger-scanner validation without desktop capture or input synthesis.</summary>
public static class TriggerBotEngineSelfTest
{
    public static async Task RunAsync()
    {
        await TestExactMatchAndPayloadAsync();
        await TestToleranceBoundaryAsync();
        await TestCooldownAsync();
        await TestDuplicateIgnoreAsync();
        await TestRestartResetsStateAsync();
    }

    private static async Task TestExactMatchAndPayloadAsync()
    {
        var settings = BaseSettings();
        await using var engine = CreateEngine(settings);
        var hits = new List<TriggerMatch>();
        engine.MatchDetected += (_, match) => hits.Add(match);

        using var gate = new ManualResetEventSlim();
        engine.Start((_, _) =>
        {
            gate.Set();
            return ValueTask.FromResult<IReadOnlyList<PixelSample>>([
                new PixelSample(2, 3, new RgbColor(255, 0, 0))
            ]);
        });

        if (!gate.Wait(1000)) throw new InvalidOperationException("Scanner did not execute a frame.");
        await WaitForAsync(() => hits.Count == 1);
        await engine.StopAsync();

        var match = hits[0];
        if (match.X != 2 || match.Y != 3 || match.Distance != 0)
            throw new InvalidOperationException("Exact-match payload was incorrect.");
    }

    private static async Task TestToleranceBoundaryAsync()
    {
        var settings = BaseSettings();
        settings.TBTolerance = 3;
        await using var engine = CreateEngine(settings);
        var hits = 0;
        engine.MatchDetected += (_, _) => hits++;

        engine.Start((_, _) => ValueTask.FromResult<IReadOnlyList<PixelSample>>([
            new PixelSample(1, 1, new RgbColor(254, 255, 254)) // distance 2: inside tolerance
        ]));
        await WaitForAsync(() => hits == 1);
        await engine.StopAsync();
    }

    private static async Task TestCooldownAsync()
    {
        var settings = BaseSettings();
        settings.TBPressCooldown = 250;
        settings.TBDuplicateIgnore = 0;
        await using var engine = CreateEngine(settings);
        var hits = 0;
        engine.MatchDetected += (_, _) => hits++;

        engine.Start((_, _) => ValueTask.FromResult<IReadOnlyList<PixelSample>>([
            new PixelSample(1, 1, new RgbColor(255, 0, 0)),
            new PixelSample(4, 4, new RgbColor(255, 0, 0))
        ]));
        await WaitForAsync(() => hits == 1);
        await Task.Delay(50);
        if (hits != 1) throw new InvalidOperationException("Cooldown failed to suppress a second match.");
        await engine.StopAsync();
    }

    private static async Task TestDuplicateIgnoreAsync()
    {
        var settings = BaseSettings();
        settings.TBPressCooldown = 0;
        settings.TBDuplicateIgnore = 250;
        await using var engine = CreateEngine(settings);
        var hits = 0;
        engine.MatchDetected += (_, _) => hits++;

        engine.Start((_, _) => ValueTask.FromResult<IReadOnlyList<PixelSample>>([
            new PixelSample(5, 6, new RgbColor(255, 0, 0))
        ]));
        await WaitForAsync(() => hits == 1);
        await Task.Delay(50);
        if (hits != 1) throw new InvalidOperationException("Duplicate-position filtering failed.");
        await engine.StopAsync();
    }

    private static async Task TestRestartResetsStateAsync()
    {
        var settings = BaseSettings();
        settings.TBPressCooldown = 1000;
        settings.TBDuplicateIgnore = 1000;
        await using var engine = CreateEngine(settings);
        var hits = 0;
        engine.MatchDetected += (_, _) => hits++;

        engine.Start((_, _) => ValueTask.FromResult<IReadOnlyList<PixelSample>>([
            new PixelSample(7, 7, new RgbColor(255, 0, 0))
        ]));
        await WaitForAsync(() => hits == 1);
        await engine.StopAsync();

        engine.Start((_, _) => ValueTask.FromResult<IReadOnlyList<PixelSample>>([
            new PixelSample(7, 7, new RgbColor(255, 0, 0))
        ]));
        await WaitForAsync(() => hits == 2);
        await engine.StopAsync();
    }

    private static TriggerBotEngine CreateEngine(AppSettings settings)
    {
        var engine = new TriggerBotEngine();
        engine.Configure(settings, new ScanRegion(0, 0, 10, 10));
        return engine;
    }

    private static AppSettings BaseSettings() => new()
    {
        TBTargetColorHex = "#FF0000",
        TBTolerance = 0,
        TBPressCooldown = 100,
        TBDuplicateIgnore = 50
    };

    private static async Task WaitForAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(1);
        while (!condition())
        {
            if (DateTime.UtcNow >= deadline)
                throw new InvalidOperationException("Scanner self-test timed out.");
            await Task.Delay(5).ConfigureAwait(false);
        }
    }
}
