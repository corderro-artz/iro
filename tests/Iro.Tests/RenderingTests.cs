using Iro.Internal;
using Iro.Rendering;
using Xunit;

namespace Iro.Tests;

public class RenderingTests
{
    [Fact]
    public void AnsiRenderer_ForegroundColor_EmitsSequence()
    {
        var tokens = new StyleToken[]
        {
            new(TokenType.StylePush, Style: new Style(Foreground: Color.Red)),
            new(TokenType.Literal,   Text: "hello"),
            new(TokenType.StylePop)
        };

        var result = new AnsiRenderer().Render(tokens);

        Assert.Contains("\e[38;2;255;0;0m", result);
        Assert.Contains("hello", result);
        Assert.EndsWith("\e[0m", result);
    }

    [Fact]
    public void AnsiRenderer_BoldUnderline_EmitsSequences()
    {
        var tokens = new StyleToken[]
        {
            new(TokenType.StylePush, Style: new Style(Bold: true, Underline: true)),
            new(TokenType.Literal,   Text: "x"),
            new(TokenType.StylePop)
        };

        var result = new AnsiRenderer().Render(tokens);

        Assert.Contains("\e[1m", result);
        Assert.Contains("\e[4m", result);
    }

    [Fact]
    public void AnsiRenderer_NoStyle_NoReset()
    {
        var tokens = new StyleToken[] { new(TokenType.Literal, Text: "plain") };
        var result = new AnsiRenderer().Render(tokens);
        Assert.Equal("plain", result);
    }

    [Fact]
    public void AnsiRenderer_NestedColors_ResetsAndReapplies()
    {
        var tokens = new StyleToken[]
        {
            new(TokenType.StylePush, Style: new Style(Foreground: Color.Red)),
            new(TokenType.Literal,   Text: "A"),
            new(TokenType.StylePush, Style: new Style(Foreground: Color.Green)),
            new(TokenType.Literal,   Text: "B"),
            new(TokenType.StylePop),
            new(TokenType.Literal,   Text: "C"),
            new(TokenType.StylePop)
        };

        var result = new AnsiRenderer().Render(tokens);

        Assert.Contains("\e[38;2;255;0;0m", result);
        Assert.Contains("\e[38;2;0;255;0m", result);
        Assert.Contains("\e[0m", result);
        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void AnsiRenderer_MultipleTransitions_AllColored()
    {
        // [red]A[/][green]B[/]
        var tokens = new StyleToken[]
        {
            new(TokenType.StylePush, Style: new Style(Foreground: Color.Red)),
            new(TokenType.Literal,   Text: "A"),
            new(TokenType.StylePop),
            new(TokenType.StylePush, Style: new Style(Foreground: Color.Green)),
            new(TokenType.Literal,   Text: "B"),
            new(TokenType.StylePop)
        };

        var result = new AnsiRenderer().Render(tokens);

        Assert.Contains("\e[38;2;255;0;0m", result);
        Assert.Contains("\e[38;2;0;255;0m", result);
        Assert.Contains("A", result);
        Assert.Contains("B", result);
    }

    [Fact]
    public void ConsoleRenderer_WritesText_ToTextWriter()
    {
        var tokens = new StyleToken[]
        {
            new(TokenType.StylePush, Style: new Style(Foreground: Color.Red)),
            new(TokenType.Literal,   Text: "hello"),
            new(TokenType.StylePop)
        };

        using var sw = new StringWriter();
        new ConsoleRenderer().Render(tokens, sw);

        Assert.Equal("hello", sw.ToString());
    }

    [Fact]
    public void ConsoleRenderer_MultiTransition_WritesAllText()
    {
        var tokens = new StyleToken[]
        {
            new(TokenType.StylePush, Style: new Style(Foreground: Color.Red)),
            new(TokenType.Literal,   Text: "A"),
            new(TokenType.StylePop),
            new(TokenType.StylePush, Style: new Style(Foreground: Color.Green)),
            new(TokenType.Literal,   Text: "B"),
            new(TokenType.StylePop)
        };

        using var sw = new StringWriter();
        new ConsoleRenderer().Render(tokens, sw);

        Assert.Equal("AB", sw.ToString());
    }
}
