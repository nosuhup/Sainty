namespace Sainty.Models;

/// <summary>Describes a pixel-scan request without performing screen capture or input injection.</summary>
public readonly record struct ScanRegion(int X, int Y, int Width, int Height)
{
    public int Right => X + Width;
    public int Bottom => Y + Height;
}

/// <summary>Normalized target color used by the trigger scanner.</summary>
public readonly record struct RgbColor(byte R, byte G, byte B)
{
    public static bool TryParseHex(string? value, out RgbColor color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(value)) return false;
        var hex = value.Trim().TrimStart('#');
        if (hex.Length != 6 || !int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var raw)) return false;
        color = new RgbColor((byte)(raw >> 16), (byte)(raw >> 8), (byte)raw);
        return true;
    }

    public int Distance(RgbColor other) => Math.Abs(R - other.R) + Math.Abs(G - other.G) + Math.Abs(B - other.B);
}

/// <summary>Input-independent result emitted when a scan finds a matching pixel.</summary>
public readonly record struct TriggerMatch(int X, int Y, int Distance, DateTimeOffset Timestamp);
