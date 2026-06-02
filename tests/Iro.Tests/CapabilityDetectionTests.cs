using Iro.Internal;
using Xunit;

namespace Iro.Tests;

public class CapabilityDetectionTests : IDisposable
{
    private readonly Dictionary<string, string?> _saved = [];

    private void SetEnv(string name, string? value)
    {
        _saved.TryAdd(name, Environment.GetEnvironmentVariable(name));
        Environment.SetEnvironmentVariable(name, value);
    }

    public void Dispose()
    {
        foreach (var (name, value) in _saved)
            Environment.SetEnvironmentVariable(name, value);
    }

    [Fact]
    public void NoColor_Disables()
    {
        SetEnv("NO_COLOR", "1");
        SetEnv("WT_SESSION", null);
        SetEnv("COLORTERM", null);
        SetEnv("TERM", null);
        Assert.False(AnsiCapabilityDetector.Detect());
    }

    [Fact]
    public void WtSession_Enables()
    {
        SetEnv("NO_COLOR", null);
        SetEnv("WT_SESSION", "some-guid");
        Assert.True(AnsiCapabilityDetector.Detect());
    }

    [Fact]
    public void ColorTerm_Enables()
    {
        SetEnv("NO_COLOR", null);
        SetEnv("WT_SESSION", null);
        SetEnv("COLORTERM", "truecolor");
        Assert.True(AnsiCapabilityDetector.Detect());
    }

    [Theory]
    [InlineData("xterm-256color")]
    [InlineData("ansi")]
    [InlineData("color")]
    [InlineData("vt100")]
    [InlineData("screen")]
    [InlineData("tmux")]
    public void Term_KnownValue_Enables(string termValue)
    {
        SetEnv("NO_COLOR", null);
        SetEnv("WT_SESSION", null);
        SetEnv("COLORTERM", null);
        SetEnv("TERM", termValue);
        Assert.True(AnsiCapabilityDetector.Detect());
    }

    [Fact]
    public void NoKnownEnvVars_ReturnsFalse()
    {
        SetEnv("NO_COLOR", null);
        SetEnv("WT_SESSION", null);
        SetEnv("COLORTERM", null);
        SetEnv("TERM", "dumb");
        Assert.False(AnsiCapabilityDetector.Detect());
    }
}
