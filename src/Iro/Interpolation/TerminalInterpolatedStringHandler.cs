using System.Runtime.CompilerServices;
using Iro.Internal;

namespace Iro.Interpolation;

/// <summary>Custom interpolated string handler for <see cref="Terminal"/> write methods.</summary>
[InterpolatedStringHandler]
public ref struct TerminalInterpolatedStringHandler
{
    private StyleToken[] _tokens;
    private int          _count;

    /// <summary>Initialises the handler.</summary>
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

    /// <summary>Appends a formatted value.</summary>
    public void AppendFormatted<T>(T value)
        => Push(new StyleToken(TokenType.Literal, Text: value?.ToString() ?? string.Empty));

    /// <summary>Appends a formatted value with an optional color format specifier.</summary>
    public void AppendFormatted<T>(T value, string? format)
        => Push(new StyleToken(TokenType.Literal, Text: value?.ToString() ?? string.Empty));

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
}
