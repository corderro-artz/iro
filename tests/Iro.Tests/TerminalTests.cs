using Xunit;

namespace Iro.Tests;

public class TerminalTests
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

    private string CaptureWrite(Action action)
    {
        using var sw    = new StringWriter();
        var savedOut    = Console.Out;
        Console.SetOut(sw);
        try { action(); return sw.ToString(); }
        finally { Console.SetOut(savedOut); }
    }

    [Fact]
    public void Colorize_FgOnly_ReturnsColorizedText()
    {
        WithAnsiForced(() =>
        {
            var s = Terminal.Colorize("Warning", Color.Yellow);
            Assert.Contains("Warning", s.ToString());
            Assert.Contains("\e[38;2;255;255;0m", s.ToString());
        });
    }

    [Fact]
    public void Colorize_FgAndBg_BothInOutput()
    {
        WithAnsiForced(() =>
        {
            var s = Terminal.Colorize("x", Color.Red, Color.Blue);
            Assert.Contains("\e[38;2;255;0;0m", s.ToString());
            Assert.Contains("\e[48;2;0;0;255m", s.ToString());
        });
    }

    [Fact]
    public void ParseMarkup_NamedColor_ProducesAnsiOutput()
    {
        WithAnsiForced(() =>
        {
            var s = Terminal.ParseMarkup("[green]Success[/]");
            Assert.Contains("\e[38;2;0;255;0m", s.ToString());
            Assert.Contains("Success", s.ToString());
        });
    }

    [Fact]
    public void WriteLine_String_WritesMarkupToConsole()
    {
        WithAnsiForced(() =>
        {
            var output = CaptureWrite(() => Terminal.WriteLine("[red]Error[/]"));
            Assert.Contains("\e[38;2;255;0;0m", output);
            Assert.Contains("Error", output);
        });
    }

    [Fact]
    public void WriteLine_StyledText_WritesRenderedOutput()
    {
        WithAnsiForced(() =>
        {
            var styled = StyledText.Create("Running", fg: Color.Cyan, bold: true);
            var output = CaptureWrite(() => Terminal.WriteLine(styled));
            Assert.Contains("Running", output);
            Assert.Contains("\e[38;2;0;255;255m", output);
            Assert.Contains("\e[1m", output);
        });
    }

    [Fact]
    public void Options_EnableAnsiFalse_NoEscapeSequences()
    {
        var savedAnsi     = Terminal.Options.EnableAnsi;
        var savedDetect   = Terminal.Options.DetectAnsiAutomatically;
        var savedFallback = Terminal.Options.EnableConsoleFallback;
        Terminal.Options.EnableAnsi = false;
        Terminal.Options.DetectAnsiAutomatically = false;
        Terminal.Options.EnableConsoleFallback = false;
        try
        {
            var output = CaptureWrite(() => Terminal.Write("[red]test[/]"));
            Assert.Equal("test", output);
            Assert.DoesNotContain("\e[", output);
        }
        finally
        {
            Terminal.Options.EnableAnsi = savedAnsi;
            Terminal.Options.DetectAnsiAutomatically = savedDetect;
            Terminal.Options.EnableConsoleFallback = savedFallback;
        }
    }
}
