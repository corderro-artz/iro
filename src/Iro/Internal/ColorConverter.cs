namespace Iro.Internal;

internal static class ColorConverter
{
    // Standard terminal RGB approximations for each ConsoleColor
    private static readonly (ConsoleColor Color, int R, int G, int B)[] s_table =
    [
        (ConsoleColor.Black,        0,   0,   0),
        (ConsoleColor.DarkBlue,     0,   0, 128),
        (ConsoleColor.DarkGreen,    0, 128,   0),
        (ConsoleColor.DarkCyan,     0, 128, 128),
        (ConsoleColor.DarkRed,    128,   0,   0),
        (ConsoleColor.DarkMagenta, 128,  0, 128),
        (ConsoleColor.DarkYellow,  128, 128,   0),
        (ConsoleColor.Gray,        192, 192, 192),
        (ConsoleColor.DarkGray,    128, 128, 128),
        (ConsoleColor.Blue,          0,   0, 255),
        (ConsoleColor.Green,         0, 255,   0),
        (ConsoleColor.Cyan,          0, 255, 255),
        (ConsoleColor.Red,         255,   0,   0),
        (ConsoleColor.Magenta,     255,   0, 255),
        (ConsoleColor.Yellow,      255, 255,   0),
        (ConsoleColor.White,       255, 255, 255),
    ];

    internal static ConsoleColor ToNearestConsoleColor(Color color)
    {
        var best     = ConsoleColor.Black;
        var bestDist = int.MaxValue;

        foreach (var (cc, r, g, b) in s_table)
        {
            var dr = color.R - r;
            var dg = color.G - g;
            var db = color.B - b;
            var dist = dr * dr + dg * dg + db * db;
            if (dist < bestDist) { bestDist = dist; best = cc; }
        }

        return best;
    }
}
