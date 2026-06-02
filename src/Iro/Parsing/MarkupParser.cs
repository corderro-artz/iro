using System.Text;
using Iro.Internal;

namespace Iro.Parsing;

/// <summary>Single-pass markup parser. Never throws; invalid tags render literally.</summary>
internal static class MarkupParser
{
    internal static StyleToken[] Parse(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty) return [];

        var tokens  = new List<StyleToken>();
        var literal = new StringBuilder();
        int i       = 0;

        while (i < input.Length)
        {
            char c = input[i];

            if (c == '[')
            {
                // Escape: [[ → [
                if (i + 1 < input.Length && input[i + 1] == '[')
                {
                    literal.Append('[');
                    i += 2;
                    continue;
                }

                // Find closing ]
                int close = input[i..].IndexOf(']');
                if (close < 0)
                {
                    literal.Append(c);
                    i++;
                    continue;
                }

                int closeAbs = i + close;
                var tag = input[(i + 1)..closeAbs].ToString();

                if (tag == "/")
                {
                    FlushLiteral(tokens, literal);
                    tokens.Add(new StyleToken(TokenType.StylePop));
                    i = closeAbs + 1;
                    continue;
                }

                if (TryParseColorTag(tag, out var color))
                {
                    FlushLiteral(tokens, literal);
                    tokens.Add(new StyleToken(TokenType.StylePush, Style: new Style(Foreground: color)));
                    i = closeAbs + 1;
                    continue;
                }

                // Invalid tag — render literally
                literal.Append('[');
                literal.Append(tag);
                literal.Append(']');
                i = closeAbs + 1;
                continue;
            }

            if (c == ']' && i + 1 < input.Length && input[i + 1] == ']')
            {
                literal.Append(']');
                i += 2;
                continue;
            }

            literal.Append(c);
            i++;
        }

        FlushLiteral(tokens, literal);
        return [.. tokens];
    }

    private static bool TryParseColorTag(string tag, out Color color)
    {
        color = default;
        if (string.IsNullOrEmpty(tag)) return false;

        // Hex color: #RRGGBB or #RGB
        if (tag[0] == '#')
        {
            try { color = Color.FromHex(tag); return true; }
            catch (FormatException) { return false; }
        }

        var lower = tag.ToLowerInvariant();
        var matched = lower switch
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

    private static void FlushLiteral(List<StyleToken> tokens, StringBuilder sb)
    {
        if (sb.Length == 0) return;
        tokens.Add(new StyleToken(TokenType.Literal, Text: sb.ToString()));
        sb.Clear();
    }
}
