using Iro.Internal;

namespace Iro.Rendering;

internal sealed class ConsoleRenderer
{
    internal void Render(StyleToken[] tokens, TextWriter output)
    {
        var savedFg = Console.ForegroundColor;
        var savedBg = Console.BackgroundColor;
        var stack   = new Stack<Style>();

        try
        {
            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.Literal:
                        output.Write(token.Text);
                        break;

                    case TokenType.StylePush:
                        stack.Push(token.Style);
                        Apply(Merge(stack));
                        break;

                    case TokenType.StylePop:
                        if (stack.Count > 0) stack.Pop();
                        if (stack.Count > 0)
                            Apply(Merge(stack));
                        else
                        {
                            Console.ForegroundColor = savedFg;
                            Console.BackgroundColor = savedBg;
                        }
                        break;
                }
            }
        }
        finally
        {
            Console.ForegroundColor = savedFg;
            Console.BackgroundColor = savedBg;
        }
    }

    private static void Apply(Style style)
    {
        if (style.Foreground.HasValue)
            Console.ForegroundColor = style.Foreground.Value.ToNearestConsoleColor();
        if (style.Background.HasValue)
            Console.BackgroundColor = style.Background.Value.ToNearestConsoleColor();
    }

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
