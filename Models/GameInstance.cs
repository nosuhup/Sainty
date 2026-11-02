namespace Sainty.Models;

public sealed record GameInstance(string Name, int ProcessId, DateTime DetectedAt)
{
    public override string ToString() => $"{Name}  ·  PID {ProcessId}";
}
