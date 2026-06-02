namespace Iro.Internal;

internal readonly record struct Style(
    Color? Foreground = null,
    Color? Background = null,
    bool Bold = false,
    bool Underline = false);
