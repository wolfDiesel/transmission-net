using Moq;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Tests.Handlers;

public class GetTorrentsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRpcFails_Propagates()
    {
        var settings = new AppSettings(
            new DaemonConnection("127.0.0.1", 9091, "/transmission/rpc", "", ""),
            new UiSettings(3, 1280, 800, TorrentTableSettings.CreateDefault(), UiColorSchemes.Default, UiAppearances.Default));

        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        var client = new Mock<ITransmissionClient>();
        client.Setup(c => c.GetTorrentsAsync(It.IsAny<DaemonConnection>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DaemonConnectionException("RPC failed"));

        var handler = new GetTorrentsHandler(store.Object, client.Object);

        await Assert.ThrowsAsync<DaemonConnectionException>(() => handler.HandleAsync());
    }
}
