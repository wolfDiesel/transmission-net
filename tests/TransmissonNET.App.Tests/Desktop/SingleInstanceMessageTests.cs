using TransmissonNET.App.Desktop;
using Xunit;

namespace TransmissonNET.App.Tests.Desktop;

public class SingleInstanceMessageTests
{
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
}
