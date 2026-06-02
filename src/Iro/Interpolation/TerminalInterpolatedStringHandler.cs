using System.Runtime.CompilerServices;
using Iro.Internal;

namespace Iro.Interpolation;

/// <summary>
/// Custom interpolated string handler for <see cref="Terminal.Write(string)"/> and <see cref="Terminal.WriteLine(string)"/>.
/// Captures string segments and formatted values as <see cref="StyleToken"/> entries, passing them
/// through the shared rendering pipeline without building an intermediate string.
/// </summary>
[InterpolatedStringHandler]
public ref struct TerminalInterpolatedStringHandler
{
    private StyleToken[] _tokens;
    private int          _count;

    /// <summary>Initialises the handler with capacity hints from the compiler.</summary>
    public TerminalInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _tokens = new StyleToken[formattedCount * 3 + formattedCount + 4];
        _count  = 0;
    }

    /// <summary>Appends a literal string segment.</summary>
    public void AppendLiteral(string value)
    {
        if (!string.IsNullOrEmpty(value))
            Push(new StyleToken(TokenType.Literal, Text: value));
    }

    /// <summary>Appends a formatted value as a literal token.</summary>
    public void AppendFormatted<T>(T value)
        => Push(new StyleToken(TokenType.Literal, Text: value?.ToString() ?? string.Empty));

    /// <summary>
    /// Appends a formatted value with an optional color format specifier.
    /// Supported formats: named colors (<c>red</c>, <c>green</c>, <c>yellow</c>, <c>blue</c>,
    /// <c>magenta</c>, <c>cyan</c>, <c>white</c>, <c>black</c>, <c>gray</c>) and hex (<c>#FF8800</c>).
    /// Unrecognised format specifiers are ignored and the value is rendered as plain text.
    /// </summary>
    public void AppendFormatted<T>(T value, string? format)
    {
        if (format is not null && TryParseColor(format, out var color))
        {
            Push(new StyleToken(TokenType.StylePush, Style: new Style(Foreground: color)));
            Push(new StyleToken(TokenType.Literal,   Text:  value?.ToString() ?? string.Empty));
            Push(new StyleToken(TokenType.StylePop));
        }
        else
        {
            Push(new StyleToken(TokenType.Literal, Text: value?.ToString() ?? string.Empty));
        }
    }

    internal StyleToken[] GetTokens() => _tokens[.._count];

    private void Push(StyleToken token)
    {
        if (_count == _tokens.Length) Grow();
        _tokens[_count++] = token;
    }

    private void Grow()
    {
        var bigger = new StyleToken[_tokens.Length * 2];
        _tokens.CopyTo(bigger, 0);
        _tokens = bigger;
    }

    private static bool TryParseColor(string format, out Color color)
    {
        if (format.StartsWith('#'))
        {
            try { color = Color.FromHex(format); return true; }
            catch (FormatException) { color = default; return false; }
        }

        var matched = format.ToLowerInvariant() switch
        {
            "black"   => (true, Color.Black),
            "red"     => (true, Color.Red),
            "green"   => (true, Color.Green),
            "yellow"  => (true, Color.Yellow),
            "blue"    => (true, Color.Blue),
            "magenta" => (true, Color.Magenta),
            "cyan"    => (true, Color.Cyan),
            "white"   => (true, Color.White),
            "gray"    => (true, Color.Gray),
            _         => (false, default(Color))
        };

        color = matched.Item2;
        return matched.Item1;
    }
}
