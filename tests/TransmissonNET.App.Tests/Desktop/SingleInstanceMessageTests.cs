using TransmissonNET.Desktop;
using Xunit;

namespace TransmissonNET.App.Tests.Desktop;

public class SingleInstanceMessageTests
{
    [Fact]
    public void FormatOpenTorrent_RoundTripsThroughTryParse()
    {
        var message = SingleInstanceMessage.FormatOpenTorrent("/tmp/a.torrent");

        Assert.True(SingleInstanceMessage.TryParse(message, out var path, out var activateOnly));
        Assert.Equal("/tmp/a.torrent", path);
        Assert.False(activateOnly);
    }

    [Fact]
    public void TryParse_OpenCommand_ReturnsTorrentPath()
    {
        Assert.True(SingleInstanceMessage.TryParse("OPEN:/tmp/a.torrent", out var path, out var activateOnly));
        Assert.Equal("/tmp/a.torrent", path);
        Assert.False(activateOnly);
    }

    [Fact]
    public void TryParse_ActivateCommand_ActivatesOnly()
    {
        Assert.True(SingleInstanceMessage.TryParse(SingleInstanceMessage.ActivateCommand, out var path, out var activateOnly));
        Assert.Null(path);
        Assert.True(activateOnly);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParse_EmptyMessage_ActivatesOnly(string? line)
    {
        Assert.True(SingleInstanceMessage.TryParse(line, out var path, out var activateOnly));
        Assert.Null(path);
        Assert.True(activateOnly);
    }

    [Theory]
    [InlineData("OPEN:")]
    [InlineData("OPEN:   ")]
    public void TryParse_OpenWithoutPath_ActivatesOnly(string line)
    {
        Assert.True(SingleInstanceMessage.TryParse(line, out var path, out var activateOnly));
        Assert.Null(path);
        Assert.True(activateOnly);
    }

    [Theory]
    [InlineData("UNKNOWN")]
    [InlineData("OPEN")]
    [InlineData("CLOSE:/tmp/a.torrent")]
    public void TryParse_InvalidMessage_ReturnsFalse(string line)
    {
        Assert.False(SingleInstanceMessage.TryParse(line, out _, out _));
    }
}
