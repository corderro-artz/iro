using Iro.Internal;

namespace Iro;

/// <summary>Immutable truecolor value supporting RGB, ARGB, HEX, and HSV creation.</summary>
public readonly record struct Color(byte R, byte G, byte B, byte A = 255)
{
    /// <summary>Opaque black.</summary>
    public static readonly Color Black   = new(0,   0,   0);
    /// <summary>Opaque white.</summary>
    public static readonly Color White   = new(255, 255, 255);
    /// <summary>Opaque red.</summary>
    public static readonly Color Red     = new(255, 0,   0);
    /// <summary>Opaque green.</summary>
    public static readonly Color Green   = new(0,   255, 0);
    /// <summary>Opaque blue.</summary>
    public static readonly Color Blue    = new(0,   0,   255);
    /// <summary>Opaque yellow.</summary>
    public static readonly Color Yellow  = new(255, 255, 0);
    /// <summary>Opaque cyan.</summary>
    public static readonly Color Cyan    = new(0,   255, 255);
    /// <summary>Opaque magenta.</summary>
    public static readonly Color Magenta = new(255, 0,   255);
    /// <summary>Medium gray.</summary>
    public static readonly Color Gray    = new(128, 128, 128);

    /// <summary>Creates an opaque color from red, green, and blue components.</summary>
    public static Color FromRgb(byte r, byte g, byte b) => new(r, g, b);

    /// <summary>Creates a color from alpha, red, green, and blue components.</summary>
    public static Color FromArgb(byte a, byte r, byte g, byte b) => new(r, g, b, a);

    /// <summary>
    /// Creates a color from a hex string.
    /// Supported formats: #RGB, RGB, #RRGGBB, RRGGBB, #RRGGBBAA, RRGGBBAA.
    /// </summary>
    /// <exception cref="FormatException">Thrown when <paramref name="hex"/> is not a recognised format.</exception>
    public static Color FromHex(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            throw new FormatException("Hex color string is empty.");

        ReadOnlySpan<char> span = hex.AsSpan();
        if (span[0] == '#') span = span[1..];

        return span.Length switch
        {
            3  => new Color(ExpandNibble(span[0]), ExpandNibble(span[1]), ExpandNibble(span[2])),
            6  => new Color(ParseByte(span, 0), ParseByte(span, 2), ParseByte(span, 4)),
            8  => new Color(ParseByte(span, 0), ParseByte(span, 2), ParseByte(span, 4), ParseByte(span, 6)),
            _  => throw new FormatException($"Unrecognised hex color format: '{hex}'.")
        };
    }

    /// <summary>Creates a color from hue (0–360), saturation (0–1), and value (0–1).</summary>
    public static Color FromHsv(double hue, double saturation, double value)
    {
        if (saturation == 0)
        {
            var v = (byte)(value * 255);
            return new Color(v, v, v);
        }

        double h = hue % 360 / 60;
        int    i = (int)h;
        double f = h - i;
        double p = value * (1 - saturation);
        double q = value * (1 - saturation * f);
        double t = value * (1 - saturation * (1 - f));

        var (r, g, b) = i switch
        {
            0 => (value, t, p),
            1 => (q, value, p),
            2 => (p, value, t),
            3 => (p, q, value),
            4 => (t, p, value),
            _ => (value, p, q)
        };

        return new Color((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    /// <summary>Returns the ANSI truecolor foreground escape sequence for this color.</summary>
    public string ToAnsiForeground() => $"\e[38;2;{R};{G};{B}m";

    /// <summary>Returns the ANSI truecolor background escape sequence for this color.</summary>
    public string ToAnsiBackground() => $"\e[48;2;{R};{G};{B}m";

    /// <summary>Maps this color to the nearest <see cref="ConsoleColor"/> using minimum squared RGB distance.</summary>
    public ConsoleColor ToNearestConsoleColor() => ColorConverter.ToNearestConsoleColor(this);

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static byte ExpandNibble(char c)
    {
        int v = HexDigit(c);
        return (byte)(v << 4 | v);
    }

    private static byte ParseByte(ReadOnlySpan<char> span, int offset)
        => (byte)(HexDigit(span[offset]) << 4 | HexDigit(span[offset + 1]));

    private static int HexDigit(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        >= 'a' and <= 'f' => c - 'a' + 10,
        >= 'A' and <= 'F' => c - 'A' + 10,
        _ => throw new FormatException($"Invalid hex digit: '{c}'.")
    };
}
