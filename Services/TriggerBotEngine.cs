using Sainty.Models;

namespace Sainty.Services;

/// <summary>
/// Trigger detection pipeline. It consumes caller-provided pixel data and emits matches;
/// it deliberately does not capture the desktop or synthesize input events.
/// </summary>
public sealed class TriggerBotEngine : IAsyncDisposable
{
    private readonly object _gate = new();
    private CancellationTokenSource? _cts;
    private Task? _worker;
    private RgbColor _target;
    private int _tolerance;
    private TimeSpan _cooldown;
    private TimeSpan _duplicateIgnore;
    private ScanRegion _region;
    private DateTimeOffset _lastMatch = DateTimeOffset.MinValue;
    private TriggerMatch? _lastPosition;

    public event EventHandler<TriggerMatch>? MatchDetected;
    public bool IsRunning => _worker is { IsCompleted: false };

    public void Configure(AppSettings settings, ScanRegion region)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (!RgbColor.TryParseHex(settings.TBTargetColorHex, out var target))
            throw new ArgumentException("TBTargetColorHex must be a six-digit RGB value.", nameof(settings));
        if (settings.TBTolerance < 0) throw new ArgumentOutOfRangeException(nameof(settings.TBTolerance));
        if (settings.TBPressCooldown < 0) throw new ArgumentOutOfRangeException(nameof(settings.TBPressCooldown));
        if (settings.TBDuplicateIgnore < 0) throw new ArgumentOutOfRangeException(nameof(settings.TBDuplicateIgnore));
        if (region.Width <= 0 || region.Height <= 0) throw new ArgumentOutOfRangeException(nameof(region));

        lock (_gate)
        {
            _target = target;
            _tolerance = settings.TBTolerance;
            _cooldown = TimeSpan.FromMilliseconds(settings.TBPressCooldown);
            _duplicateIgnore = TimeSpan.FromMilliseconds(settings.TBDuplicateIgnore);
            _region = region;
            ResetMatchStateLocked();
        }
    }

    /// <summary>Starts the scheduling loop. ScanFrame must be supplied by the host.</summary>
    public void Start(Func<ScanRegion, CancellationToken, ValueTask<IReadOnlyList<PixelSample>>> scanFrame)
    {
        ArgumentNullException.ThrowIfNull(scanFrame);
        lock (_gate)
        {
            if (IsRunning) return;
            ResetMatchStateLocked();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            _worker = Task.Run(() => LoopAsync(scanFrame, token), token);
        }
    }

    private async Task LoopAsync(Func<ScanRegion, CancellationToken, ValueTask<IReadOnlyList<PixelSample>>> scanFrame, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            ScanRegion region;
            lock (_gate) region = _region;

            IReadOnlyList<PixelSample> pixels;
            try
            {
                pixels = await scanFrame(region, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }

            foreach (var pixel in pixels)
            {
                RgbColor target;
                int tolerance;
                TimeSpan cooldown;
                TimeSpan duplicateIgnore;
                lock (_gate)
                {
                    target = _target;
                    tolerance = _tolerance;
                    cooldown = _cooldown;
                    duplicateIgnore = _duplicateIgnore;
                }

                var distance = pixel.Color.Distance(target);
                if (distance > tolerance) continue;

                var now = DateTimeOffset.UtcNow;
                lock (_gate)
                {
                    if (now - _lastMatch < cooldown) continue;
                    if (_lastPosition is { } last && last.X == pixel.X && last.Y == pixel.Y && now - last.Timestamp < duplicateIgnore)
                        continue;

                    var match = new TriggerMatch(pixel.X, pixel.Y, distance, now);
                    _lastMatch = now;
                    _lastPosition = match;
                    MatchDetected?.Invoke(this, match);
                }
                break;
            }

            await Task.Yield();
        }
    }

    private void ResetMatchStateLocked()
    {
        _lastMatch = DateTimeOffset.MinValue;
        _lastPosition = null;
    }

    public async ValueTask DisposeAsync() => await StopAsync().ConfigureAwait(false);

    public async ValueTask StopAsync()
    {
        CancellationTokenSource? cts;
        Task? worker;
        lock (_gate)
        {
            cts = _cts;
            worker = _worker;
            _cts = null;
            _worker = null;
            ResetMatchStateLocked();
        }
        if (cts is null) return;
        cts.Cancel();
        try { if (worker is not null) await worker.ConfigureAwait(false); }
        catch (OperationCanceledException) { }
        finally { cts.Dispose(); }
    }
}

public readonly record struct PixelSample(int X, int Y, RgbColor Color);
