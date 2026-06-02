namespace Iro;

/// <summary>Runtime configuration for ANSI and ConsoleColor rendering behaviour.</summary>
public sealed class TerminalOptions
{
    /// <summary>
    /// When <see langword="true"/> and <see cref="DetectAnsiAutomatically"/> is <see langword="false"/>,
    /// ANSI escape sequences are emitted unconditionally.
    /// When <see cref="DetectAnsiAutomatically"/> is <see langword="true"/>, this acts as a gate:
    /// ANSI is only used when both this property and auto-detection agree.
    /// Set to <see langword="false"/> to force-disable ANSI regardless of environment.
    /// </summary>
    public bool EnableAnsi { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, <see cref="EnableAnsi"/> is combined with automatic environment
    /// detection (NO_COLOR, WT_SESSION, COLORTERM, TERM). When <see langword="false"/>,
    /// <see cref="EnableAnsi"/> is the sole control.
    /// </summary>
    public bool DetectAnsiAutomatically { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, output falls back to <see cref="ConsoleColor"/> when ANSI
    /// is unavailable. When <see langword="false"/>, no color is applied in non-ANSI environments.
    /// </summary>
    public bool EnableConsoleFallback { get; set; } = true;
}
