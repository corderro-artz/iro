namespace Iro.Internal;

internal readonly record struct StyleToken(
    TokenType Type,
    string? Text = null,
    Style Style = default);
