using TransmissonNET.Application;

namespace TransmissonNET.Application.Tests;

public class PendingTorrentLaunchStoreTests
{
    [Fact]
    public void TakePendingPath_ReturnsPathOnce()
    {
        var store = new PendingTorrentLaunchStore();
        store.SetPendingPath("/tmp/sample.torrent");

        Assert.Equal("/tmp/sample.torrent", store.TakePendingPath());
        Assert.Null(store.TakePendingPath());
    }

    [Fact]
    public void PeekPendingPath_DoesNotClearPath()
    {
        var store = new PendingTorrentLaunchStore();
        store.SetPendingPath("/tmp/sample.torrent");

        Assert.Equal("/tmp/sample.torrent", store.PeekPendingPath());
        Assert.Equal("/tmp/sample.torrent", store.TakePendingPath());
    }
}
