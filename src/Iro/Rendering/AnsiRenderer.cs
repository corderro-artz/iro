using System.Text;
using Iro.Internal;

namespace Iro.Rendering;

internal sealed class AnsiRenderer
{
    internal string Render(StyleToken[] tokens)
    {
        var sb            = new StringBuilder();
        var stack         = new Stack<Style>();
        bool emittedStyle = false;

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.Literal:
                    sb.Append(token.Text);
                    break;

                case TokenType.StylePush:
                    stack.Push(token.Style);
                    Emit(sb, token.Style, ref emittedStyle);
                    break;

                case TokenType.StylePop:
                    if (stack.Count > 0) stack.Pop();
                    sb.Append("\e[0m");
                    if (stack.Count > 0)
                    {
                        var merged = Merge(stack);
                        emittedStyle = false;
                        Emit(sb, merged, ref emittedStyle);
                    }
                    else
                    {
                        emittedStyle = false;
                    }
                    break;
            }
        }

        if (emittedStyle) sb.Append("\e[0m");
        return sb.ToString();
    }

    private static void Emit(StringBuilder sb, Style style, ref bool emitted)
    {
        if (style.Foreground.HasValue) { sb.Append(style.Foreground.Value.ToAnsiForeground()); emitted = true; }
        if (style.Background.HasValue) { sb.Append(style.Background.Value.ToAnsiBackground()); emitted = true; }
        if (style.Bold)      { sb.Append("\e[1m"); emitted = true; }
        if (style.Underline) { sb.Append("\e[4m"); emitted = true; }
    }

    // Merge all stack styles bottom-to-top; later entries override for fg/bg.
    private static Style Merge(Stack<Style> stack)
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
