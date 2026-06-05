using TransmissonNET.Application.Torrents;

namespace TransmissonNET.Application.Tests.Torrents;

public sealed class TorrentNameWildcardFilterTests
{
    [Theory]
    [InlineData("Windows 11 ISO", "w", true)]
    [InlineData("Ubuntu 22.04 Desktop", "ubuntu", true)]
    [InlineData("Ubuntu 22.04 Desktop", "WIN", false)]
    [InlineData("Ubuntu 22.04 Desktop", "ubuntu*", true)]
    [InlineData("Ubuntu 22.04 Desktop", "*desktop", true)]
    [InlineData("S01E02", "S0?E*", true)]
    [InlineData("S01E02", "S02E02", false)]
    [InlineData("test", "", true)]
    [InlineData("test", "   ", true)]
    [InlineData("", "w", false)]
    [InlineData("My Windows ISO", "windows", true)]
    public void IsMatch_WildcardAndPlainText(string name, string pattern, bool expected) =>
        Assert.Equal(expected, TorrentNameWildcardFilter.IsMatch(name, pattern));
}
