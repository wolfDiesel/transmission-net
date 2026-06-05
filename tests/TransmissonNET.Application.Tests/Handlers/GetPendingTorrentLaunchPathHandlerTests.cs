using TransmissonNET.Application;
using TransmissonNET.Application.Handlers;

namespace TransmissonNET.Application.Tests.Handlers;

public class GetPendingTorrentLaunchPathHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenConsumeTrue_ReturnsPathOnce()
    {
        var store = new PendingTorrentLaunchStore();
        store.SetPendingPath("/tmp/sample.torrent");
        var handler = new GetPendingTorrentLaunchPathHandler(store);

        Assert.Equal("/tmp/sample.torrent", await handler.HandleAsync(consume: true));
        Assert.Null(await handler.HandleAsync(consume: true));
    }

    [Fact]
    public async Task HandleAsync_WhenConsumeFalse_PeeksWithoutClearing()
    {
        var store = new PendingTorrentLaunchStore();
        store.SetPendingPath("/tmp/sample.torrent");
        var handler = new GetPendingTorrentLaunchPathHandler(store);

        Assert.Equal("/tmp/sample.torrent", await handler.HandleAsync(consume: false));
        Assert.Equal("/tmp/sample.torrent", await handler.HandleAsync(consume: true));
    }

    [Fact]
    public async Task HandleAsync_WhenEmpty_ReturnsNull()
    {
        var handler = new GetPendingTorrentLaunchPathHandler(new PendingTorrentLaunchStore());

        Assert.Null(await handler.HandleAsync());
    }
}
