using System.Text;
using Iro.Internal;

namespace Iro.Parsing;

/// <summary>Single-pass O(n) markup parser. Never throws; invalid tags render literally.</summary>
internal static class MarkupParser
{
    internal static StyleToken[] Parse(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty) return [];

        var tokens    = new List<StyleToken>();
        var literal   = new StringBuilder();
        var tagBuffer = new StringBuilder();
        bool inTag    = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (!inTag)
            {
                if (c == '[')
                {
                    if (i + 1 < input.Length && input[i + 1] == '[')
                    {
                        literal.Append('[');
                        i++; // skip second [
                    }
                    else
                    {
                        inTag = true;
                        tagBuffer.Clear();
                    }
                }
                else if (c == ']' && i + 1 < input.Length && input[i + 1] == ']')
                {
                    literal.Append(']');
                    i++; // skip second ]
                }
                else
                {
                    literal.Append(c);
                }
            }
            else // inTag
            {
                if (c == ']')
                {
                    inTag = false;
                    var tag = tagBuffer.ToString();
                    ProcessTag(tag, tokens, literal);
                }
                else if (c == '[')
                {
                    // Nested [ means the outer [ was not a tag opener — flush as literal
                    literal.Append('[');
                    literal.Append(tagBuffer);
                    tagBuffer.Clear();
                    // Do NOT exit inTag — the current [ starts a new tag scan
                    // (inTag stays true, tagBuffer is clear)
                }
                else
                {
                    tagBuffer.Append(c);
                }
            }
        }

        // Unclosed tag at end of input — flush as literal
        if (inTag)
        {
            literal.Append('[');
            literal.Append(tagBuffer);
        }

        FlushLiteral(tokens, literal);
        return [.. tokens];
    }

    private static void ProcessTag(string tag, List<StyleToken> tokens, StringBuilder literal)
    {
        if (tag == "/")
        {
            FlushLiteral(tokens, literal);
            tokens.Add(new StyleToken(TokenType.StylePop));
            return;
        }

        if (TryParseColorTag(tag, out var color))
        {
            FlushLiteral(tokens, literal);
            tokens.Add(new StyleToken(TokenType.StylePush, Style: new Style(Foreground: color)));
            return;
        }

        // Invalid tag — render literally
        literal.Append('[');
        literal.Append(tag);
        literal.Append(']');
    }

    private static bool TryParseColorTag(string tag, out Color color)
    {
        color = default;
        if (string.IsNullOrEmpty(tag)) return false;

        if (tag[0] == '#')
        {
            try { color = Color.FromHex(tag); return true; }
            catch (FormatException) { return false; }
        }

        var matched = tag.ToLowerInvariant() switch
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
