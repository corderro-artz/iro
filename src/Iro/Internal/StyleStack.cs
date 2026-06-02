namespace Iro.Internal;

internal static class StyleStack
{
    /// <summary>Merges all styles in the stack bottom-to-top; later entries override fg/bg.</summary>
    internal static Style Merge(Stack<Style> stack)
    {
        Color? fg = null, bg = null;
        bool bold = false, underline = false;

        foreach (var s in stack.Reverse())
        {
            if (s.Foreground.HasValue) fg = s.Foreground;
            if (s.Background.HasValue) bg = s.Background;
            bold      |= s.Bold;
            underline |= s.Underline;
        }

        return new Style(fg, bg, bold, underline);
    }
}
