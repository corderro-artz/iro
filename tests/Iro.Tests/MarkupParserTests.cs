using Iro.Internal;
using Iro.Parsing;
using Xunit;

namespace Iro.Tests;

public class MarkupParserTests
{
    private static StyleToken[] Parse(string input) =>
        MarkupParser.Parse(input.AsSpan());

    [Fact]
    public void PlainText_ProducesSingleLiteralToken()
    {
        var tokens = Parse("hello");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Literal, tokens[0].Type);
        Assert.Equal("hello", tokens[0].Text);
    }

    [Fact]
    public void NamedColorTag_PushesAndPops()
    {
        var tokens = Parse("[red]text[/]");
        Assert.Equal(3, tokens.Length);
        Assert.Equal(TokenType.StylePush, tokens[0].Type);
        Assert.Equal(Color.Red, tokens[0].Style.Foreground);
        Assert.Equal(TokenType.Literal, tokens[1].Type);
        Assert.Equal("text", tokens[1].Text);
        Assert.Equal(TokenType.StylePop, tokens[2].Type);
    }

    [Fact]
    public void HexColorTag_Parses()
    {
        var tokens = Parse("[#FF8800]x[/]");
        Assert.Equal(TokenType.StylePush, tokens[0].Type);
        Assert.Equal(Color.FromHex("#FF8800"), tokens[0].Style.Foreground);
    }

    [Fact]
    public void NestedTags_ProduceCorrectTokenOrder()
    {
        var tokens = Parse("[red]A[green]B[/][/]");
        Assert.Equal(6, tokens.Length);
        Assert.Equal(TokenType.StylePush, tokens[0].Type);
        Assert.Equal(Color.Red, tokens[0].Style.Foreground);
        Assert.Equal(TokenType.Literal, tokens[1].Type);
        Assert.Equal("A", tokens[1].Text);
        Assert.Equal(TokenType.StylePush, tokens[2].Type);
        Assert.Equal(Color.Green, tokens[2].Style.Foreground);
        Assert.Equal(TokenType.Literal, tokens[3].Type);
        Assert.Equal("B", tokens[3].Text);
        Assert.Equal(TokenType.StylePop, tokens[4].Type);
        Assert.Equal(TokenType.StylePop, tokens[5].Type);
    }

    [Fact]
    public void EscapedOpenBracket_ProducesLiteral()
    {
        var tokens = Parse("[[hello");
        var text = string.Concat(tokens.Select(t => t.Text));
        Assert.Equal("[hello", text);
    }

    [Fact]
    public void EscapedCloseBracket_ProducesLiteral()
    {
        var tokens = Parse("hello]]");
        var text = string.Concat(tokens.Select(t => t.Text));
        Assert.Equal("hello]", text);
    }

    [Fact]
    public void EscapedBrackets_AroundContent_ProduceLiterals()
    {
        var tokens = Parse("[[not a tag]]");
        Assert.All(tokens, t => Assert.Equal(TokenType.Literal, t.Type));
        var text = string.Concat(tokens.Select(t => t.Text));
        Assert.Equal("[not a tag]", text);
    }

    [Fact]
    public void InvalidTag_RendersLiterally()
    {
        var tokens = Parse("[test]content");
        var text = string.Concat(tokens.Select(t => t.Text));
        Assert.Equal("[test]content", text);
    }

    [Fact]
    public void EmptyInput_ReturnsEmpty()
    {
        var tokens = MarkupParser.Parse(ReadOnlySpan<char>.Empty);
        Assert.Empty(tokens);
    }

    [Theory]
    [InlineData("[black]")]
    [InlineData("[green]")]
    [InlineData("[yellow]")]
    [InlineData("[blue]")]
    [InlineData("[magenta]")]
    [InlineData("[cyan]")]
    [InlineData("[white]")]
    [InlineData("[gray]")]
    public void AllNamedColors_Recognized(string tag)
    {
        var tokens = MarkupParser.Parse(tag.AsSpan());
        Assert.Contains(tokens, t => t.Type == TokenType.StylePush);
    }

    [Fact]
    public void UnclosedTag_NoBracket_RendersLiterally()
    {
        // "[red" — no closing bracket, treated as literal
        var tokens = Parse("[red");
        var text = string.Concat(tokens.Select(t => t.Text));
        Assert.Equal("[red", text);
    }
}
