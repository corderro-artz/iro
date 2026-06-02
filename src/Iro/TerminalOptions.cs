namespace Iro;

/// <summary>Runtime configuration for ANSI and ConsoleColor rendering behaviour.</summary>
public sealed class TerminalOptions
{
    /// <summary>When true, ANSI escape sequences may be emitted.</summary>
    public bool EnableAnsi { get; set; } = true;
    /// <summary>When true, combines EnableAnsi with automatic environment detection.</summary>
    public bool DetectAnsiAutomatically { get; set; } = true;
    /// <summary>When true, falls back to ConsoleColor when ANSI is unavailable.</summary>
    public bool EnableConsoleFallback { get; set; } = true;
}
