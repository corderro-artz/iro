using Xunit;

namespace Iro.Tests;

public class ColorTests
{
    [Fact]
    public void FromRgb_CreatesCorrectColor()
    {
        var c = Color.FromRgb(10, 20, 30);
        Assert.Equal(10, c.R); Assert.Equal(20, c.G); Assert.Equal(30, c.B); Assert.Equal(255, c.A);
    }

    [Fact]
    public void FromArgb_SetsAlpha()
    {
        var c = Color.FromArgb(128, 10, 20, 30);
        Assert.Equal(128, c.A);
        Assert.Equal(10, c.R);
        Assert.Equal(20, c.G);
        Assert.Equal(30, c.B);
    }

    [Theory]
    [InlineData("#FF8800",  255, 136,   0, 255)]
    [InlineData("FF8800",   255, 136,   0, 255)]
    [InlineData("#F80",     255, 136,   0, 255)]
    [InlineData("F80",      255, 136,   0, 255)]
    [InlineData("#AAFFAA00", 255, 170, 0, 170)]
    [InlineData("AAFFAA00", 255, 170, 0, 170)]
    public void FromHex_ParsesAllFormats(string hex, byte r, byte g, byte b, byte a)
    {
        var c = Color.FromHex(hex);
        Assert.Equal(r, c.R); Assert.Equal(g, c.G); Assert.Equal(b, c.B); Assert.Equal(a, c.A);
    }

    [Theory]
    [InlineData("")]
    [InlineData("GGGGGG")]
    [InlineData("#1234567")]
    public void FromHex_InvalidInput_ThrowsFormatException(string hex)
        => Assert.Throws<FormatException>(() => Color.FromHex(hex));

    [Fact]
    public void FromHsv_RedHue_ProducesRed()
    {
        var c = Color.FromHsv(0, 1.0, 1.0);
        Assert.Equal(255, c.R); Assert.Equal(0, c.G); Assert.Equal(0, c.B);
    }

    [Fact]
    public void FromHsv_ZeroSaturation_ProducesGray()
    {
        var c = Color.FromHsv(180, 0, 0.5);
        Assert.Equal(c.R, c.G); Assert.Equal(c.G, c.B);
    }

    [Fact]
    public void Constants_HaveExpectedValues()
    {
        Assert.Equal(new Color(0, 0, 0),     Color.Black);
        Assert.Equal(new Color(255,255,255), Color.White);
        Assert.Equal(new Color(255,0,0),     Color.Red);
        Assert.Equal(new Color(0,255,0),     Color.Green);
        Assert.Equal(new Color(0,0,255),     Color.Blue);
        Assert.Equal(new Color(255,255,0),   Color.Yellow);
        Assert.Equal(new Color(0,255,255),   Color.Cyan);
        Assert.Equal(new Color(255,0,255),   Color.Magenta);
        Assert.Equal(new Color(128,128,128), Color.Gray);
    }

    [Fact]
    public void ToAnsiForeground_ProducesCorrectSequence()
    {
        var c = Color.FromRgb(255, 128, 0);
        Assert.Equal("\e[38;2;255;128;0m", c.ToAnsiForeground());
    }

    [Fact]
    public void ToAnsiBackground_ProducesCorrectSequence()
    {
        var c = Color.FromRgb(0, 128, 255);
        Assert.Equal("\e[48;2;0;128;255m", c.ToAnsiBackground());
    }

    [Fact]
    public void ToNearestConsoleColor_Red_MapsToRed()
        => Assert.Equal(ConsoleColor.Red, Color.Red.ToNearestConsoleColor());

    [Fact]
    public void ToNearestConsoleColor_Black_MapsToBlack()
        => Assert.Equal(ConsoleColor.Black, Color.Black.ToNearestConsoleColor());

    [Fact]
    public void ToNearestConsoleColor_White_MapsToWhite()
        => Assert.Equal(ConsoleColor.White, Color.White.ToNearestConsoleColor());
}
