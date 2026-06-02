namespace Iro.Internal;

internal static class AnsiCapabilityDetector
{
    private static readonly string[] s_termKeywords = ["xterm", "ansi", "color", "vt100", "screen", "tmux"];

    /// <summary>
    /// Returns true when ANSI escape sequences are likely supported.
    /// Detection order: NO_COLOR → explicit opt-in (WT_SESSION/COLORTERM/TERM keywords) → redirected output.
    /// </summary>
    internal static bool Detect()
    {
        // Explicit opt-out always wins
        if (Environment.GetEnvironmentVariable("NO_COLOR") is not null)
            return false;

        // Explicit opt-in via well-known env vars wins over redirect check
        if (Environment.GetEnvironmentVariable("WT_SESSION") is not null)
            return true;
        if (Environment.GetEnvironmentVariable("COLORTERM") is not null)
            return true;

        // TERM heuristics also win over redirect check (an explicit TERM value signals intent)
        var term = Environment.GetEnvironmentVariable("TERM");
        if (term is not null)
            foreach (var kw in s_termKeywords)
                if (term.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    return true;

        // Redirected output with no recognisable env vars: disable ANSI
        return !Console.IsOutputRedirected;
    }
}
