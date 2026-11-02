using System.Diagnostics;
using Sainty.Models;

namespace Sainty.Services;

/// <summary>
/// Timing/scheduling core reconstructed from the original MacroEngine surface.
/// It emits abstract actions; platform input injection is intentionally kept out
/// of this clean-room milestone so the scheduler can be tested independently.
/// </summary>
public sealed class MacroEngine : IAsyncDisposable
{
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public bool IsRunning => _loop is { IsCompleted: false };
    public event EventHandler<MacroActionEventArgs>? ActionRequested;

    public bool Start(AppSettings settings)
    {
        if (IsRunning) return false;
        ArgumentNullException.ThrowIfNull(settings);
        var profile = MacroProfile.FromSettings(settings, settings.LastProfileName);
        profile.Aps = Math.Clamp(profile.Aps, 1, 1500);
        _cts = new CancellationTokenSource();
        _loop = RunAsync(profile, _cts.Token);
        return true;
    }

    public async Task StopAsync()
    {
        var cts = _cts;
        var loop = _loop;
        if (cts is null || loop is null) return;
        cts.Cancel();
        try { await loop.ConfigureAwait(false); }
        catch (OperationCanceledException) { }
        finally { cts.Dispose(); _cts = null; _loop = null; }
    }

    private async Task RunAsync(MacroProfile profile, CancellationToken token)
    {
        var interval = TimeSpan.FromSeconds(1d / profile.Aps);
        var stopwatch = Stopwatch.StartNew();
        var next = stopwatch.Elapsed;
        var random = new Random();
        while (!token.IsCancellationRequested)
        {
            ActionRequested?.Invoke(this, new MacroActionEventArgs(profile.ActionMode, profile.MacroKeyToSpam));
            var jitter = profile.HoldMode ? TimeSpan.FromMilliseconds(random.NextDouble() * 2d - 1d) : TimeSpan.Zero;
            next += interval + jitter;
            var delay = next - stopwatch.Elapsed;
            if (delay > TimeSpan.Zero) await Task.Delay(delay, token).ConfigureAwait(false);
            else await Task.Yield();
        }
    }

    public async ValueTask DisposeAsync() => await StopAsync().ConfigureAwait(false);
}

public sealed class MacroActionEventArgs(string actionMode, string action) : EventArgs
{
    public string ActionMode { get; } = actionMode;
    public string Action { get; } = action;
}
