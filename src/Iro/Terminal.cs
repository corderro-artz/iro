using Iro.Interpolation;
using Iro.Internal;
using Iro.Parsing;
using Iro.Rendering;

namespace Iro;

/// <summary>Static entry point for all terminal output operations.</summary>
public static class Terminal
{
    /// <summary>Global runtime options for ANSI detection and fallback behaviour.</summary>
    public static TerminalOptions Options { get; } = new();

    /// <summary>Writes a markup-parsed string to standard output without a trailing newline.</summary>
    public static void Write(string text)
        => Renderer.RenderMarkup(text, Console.Out);

    /// <summary>Writes a markup-parsed string to standard output followed by a newline.</summary>
    public static void WriteLine(string text)
    {
        Renderer.RenderMarkup(text, Console.Out);
        Console.WriteLine();
    }

    /// <summary>Writes a <see cref="StyledText"/> to standard output without a trailing newline.</summary>
    public static void Write(StyledText text)
        => Renderer.Render(text.Tokens, Console.Out);

    /// <summary>Writes a <see cref="StyledText"/> to standard output followed by a newline.</summary>
    public static void WriteLine(StyledText text)
    {
        Renderer.Render(text.Tokens, Console.Out);
        Console.WriteLine();
    }

    /// <summary>Writes an interpolated string to standard output without a trailing newline.</summary>
    public static void Write(TerminalInterpolatedStringHandler handler)
        => Renderer.Render(handler.GetTokens(), Console.Out);

    /// <summary>Writes an interpolated string to standard output followed by a newline.</summary>
    public static void WriteLine(TerminalInterpolatedStringHandler handler)
    {
        Renderer.Render(handler.GetTokens(), Console.Out);
        Console.WriteLine();
    }

    /// <summary>Creates a <see cref="StyledText"/> with the specified foreground color.</summary>
    public static StyledText Colorize(string text, Color fg)
        => StyledText.Create(text, fg: fg);

    /// <summary>Creates a <see cref="StyledText"/> with foreground and background colors.</summary>
    public static StyledText Colorize(string text, Color fg, Color bg)
        => StyledText.Create(text, fg: fg, bg: bg);

    /// <summary>Creates a <see cref="StyledText"/> with fully specified style attributes.</summary>
    public static StyledText Colorize(
        string  text,
        Color?  fg        = null,
        Color?  bg        = null,
        bool    bold      = false,
        bool    underline = false)
        => StyledText.Create(text, fg, bg, bold, underline);

    /// <summary>Parses <paramref name="text"/> as markup and returns a styled representation.</summary>
    public static StyledText ParseMarkup(string text)
        => new StyledText(MarkupParser.Parse((text ?? string.Empty).AsSpan()));
}
