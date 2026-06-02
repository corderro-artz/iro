using Xunit;

namespace Iro.Tests;

[Collection("Console")]
public class StyledTextTests
{
    private void WithAnsiForced(Action action)
    {
        var savedAnsi   = Terminal.Options.EnableAnsi;
        var savedDetect = Terminal.Options.DetectAnsiAutomatically;
        Terminal.Options.EnableAnsi = true;
        Terminal.Options.DetectAnsiAutomatically = false;
        try { action(); }
        finally
        {
            Terminal.Options.EnableAnsi = savedAnsi;
            Terminal.Options.DetectAnsiAutomatically = savedDetect;
        }
    }

    private void WithAnsiDisabled(Action action)
    {
        var savedAnsi      = Terminal.Options.EnableAnsi;
        var savedDetect    = Terminal.Options.DetectAnsiAutomatically;
        var savedFallback  = Terminal.Options.EnableConsoleFallback;
        Terminal.Options.EnableAnsi = false;
        Terminal.Options.DetectAnsiAutomatically = false;
        Terminal.Options.EnableConsoleFallback = false;
        try { action(); }
        finally
        {
            Terminal.Options.EnableAnsi = savedAnsi;
            Terminal.Options.DetectAnsiAutomatically = savedDetect;
            Terminal.Options.EnableConsoleFallback = savedFallback;
        }
    }

    [Fact]
    public void Create_NullText_TreatedAsEmpty()
    {
        var s = StyledText.Create(null!);
        // Should not throw and ToString should not contain null chars
        var str = s.ToString();
        Assert.DoesNotContain('\0', str);
    }

    [Fact]
    public void Create_NoStyle_ToStringReturnsPlainText()
    {
        WithAnsiDisabled(() =>
        {
            var s   = StyledText.Create("hello");
            var str = s.ToString();
            Assert.Equal("hello", str);
        });
    }

    [Fact]
    public void ToString_WithAnsiEnabled_ContainsForegroundSequence()
    {
        WithAnsiForced(() =>
        {
            var s   = StyledText.Create("hello", fg: Color.Red);
            var str = s.ToString();
            Assert.Contains("\e[38;2;255;0;0m", str);
            Assert.Contains("hello", str);
            Assert.EndsWith("\e[0m", str);
        });
    }

    [Fact]
    public void ToString_WithAnsiEnabled_ContainsBackgroundSequence()
    {
        WithAnsiForced(() =>
        {
            var s   = StyledText.Create("x", bg: Color.Blue);
            var str = s.ToString();
            Assert.Contains("\e[48;2;0;0;255m", str);
        });
    }

    [Fact]
    public void ToString_WithAnsiEnabled_Bold()
    {
        WithAnsiForced(() =>
        {
            var s   = StyledText.Create("b", bold: true);
            var str = s.ToString();
            Assert.Contains("\e[1m", str);
        });
    }

    [Fact]
    public void ToString_WithAnsiEnabled_Underline()
    {
        WithAnsiForced(() =>
        {
            var s   = StyledText.Create("u", underline: true);
            var str = s.ToString();
            Assert.Contains("\e[4m", str);
        });
    }

    [Fact]
    public void ToString_WithAnsiDisabled_ReturnsPlainText()
    {
        WithAnsiDisabled(() =>
        {
            var s   = StyledText.Create("hello", fg: Color.Red);
            var str = s.ToString();
            Assert.Equal("hello", str);
            Assert.DoesNotContain("\e[", str);
        });
    }

    [Fact]
    public void Create_AllParams_AllStylesPresent()
    {
        WithAnsiForced(() =>
        {
            var s   = StyledText.Create("text", fg: Color.Cyan, bg: Color.Black, bold: true, underline: true);
            var str = s.ToString();
            Assert.Contains("text", str);
            Assert.Contains("\e[38;2;0;255;255m", str); // cyan fg
            Assert.Contains("\e[48;2;0;0;0m", str);     // black bg
            Assert.Contains("\e[1m", str);               // bold
            Assert.Contains("\e[4m", str);               // underline
        });
    }

    [Fact]
    public void ConsoleWriteLine_Works_WithoutThrowing()
    {
        var s = StyledText.Create("hello", fg: Color.Green);
        using var sw     = new StringWriter();
        var savedOut     = Console.Out;
        Console.SetOut(sw);
        try
        {
            var ex = Record.Exception(() => Console.WriteLine(s));
            Assert.Null(ex);
        }
        finally { Console.SetOut(savedOut); }
    }

    [Fact]
    public void InterpolationFormat_Works_WithoutThrowing()
    {
        var s = StyledText.Create("hello", fg: Color.Green);
        using var sw     = new StringWriter();
        var savedOut     = Console.Out;
        Console.SetOut(sw);
        try
        {
            var ex = Record.Exception(() => Console.WriteLine($"{s}"));
            Assert.Null(ex);
        }
        finally { Console.SetOut(savedOut); }
    }
}
