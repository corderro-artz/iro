using Iro.Internal;
using Iro.Parsing;

namespace Iro.Rendering;

/// <summary>Coordinates rendering: selects ANSI or ConsoleColor path based on <see cref="Terminal"/> options.</summary>
internal static class Renderer
{
    private static readonly AnsiRenderer    s_ansi    = new();
    private static readonly ConsoleRenderer s_console = new();

    /// <summary>Renders <paramref name="tokens"/> to a string (ANSI when enabled, plain text otherwise).</summary>
    internal static string RenderToString(StyleToken[] tokens)
    {
        if (IsAnsiEnabled())
            return s_ansi.Render(tokens);

        return string.Concat(tokens
            .Where(t => t.Type == TokenType.Literal)
            .Select(t => t.Text ?? string.Empty));
    }

    /// <summary>Renders <paramref name="tokens"/> to <paramref name="output"/>.</summary>
    internal static void Render(StyleToken[] tokens, TextWriter output)
    {
        if (IsAnsiEnabled())
        {
            output.Write(s_ansi.Render(tokens));
            return;
        }

        s_console.Render(tokens, output);
    }

    /// <summary>Parses <paramref name="markup"/> and renders to <paramref name="output"/>.</summary>
    internal static void RenderMarkup(string markup, TextWriter output)
        => Render(MarkupParser.Parse((markup ?? string.Empty).AsSpan()), output);

    private static bool IsAnsiEnabled()
    {
        var opts = Terminal.Options;
        if (!opts.DetectAnsiAutomatically)
            return opts.EnableAnsi;
        return opts.EnableAnsi && AnsiCapabilityDetector.Detect();
    }
}
