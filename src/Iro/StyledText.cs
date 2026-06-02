using Iro.Internal;

namespace Iro;

/// <summary>Immutable styled text that renders through the shared token pipeline.</summary>
public readonly record struct StyledText
{
    internal readonly StyleToken[] Tokens;
    internal StyledText(StyleToken[] tokens) => Tokens = tokens;

    /// <summary>Creates a <see cref="StyledText"/> from a string and optional style attributes.</summary>
    public static StyledText Create(
        string  text,
        Color?  fg        = null,
        Color?  bg        = null,
        bool    bold      = false,
        bool    underline = false)
    {
        var style = new Style(fg, bg, bold, underline);
        return new StyledText(
        [
            new StyleToken(TokenType.StylePush, Style: style),
            new StyleToken(TokenType.Literal,   Text:  text ?? string.Empty),
            new StyleToken(TokenType.StylePop)
        ]);
    }

    /// <summary>Returns the rendered string, using ANSI sequences when available.</summary>
    public override string ToString() => Rendering.Renderer.RenderToString(Tokens ?? []);
}
