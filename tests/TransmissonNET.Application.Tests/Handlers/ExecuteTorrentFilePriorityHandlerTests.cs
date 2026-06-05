using Moq;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Tests.Handlers;

public class ExecuteTorrentFilePriorityHandlerTests
{
    private static readonly AppSettings Settings = new(
        new DaemonConnection("127.0.0.1", 9091, "/transmission/rpc", "", ""),
        new UiSettings(3, 1280, 800, TorrentTableSettings.CreateDefault()));

    [Fact]
    public async Task HandleAsync_SetFilePriority_CallsTransmissionClient()
    {
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Settings);

        var client = new Mock<ITransmissionClient>();
        var handler = new ExecuteTorrentActionHandler(store.Object, client.Object);
        var dto = new TorrentActionDto(
            "set-file-priority",
            [7],
            Priority: "high",
            FileIndices: [2, 3]);

        await handler.HandleAsync(dto);

        client.Verify(c => c.SetTorrentFilePriorityAsync(
            Settings.Daemon,
            7,
            dto.FileIndices!,
            TorrentBandwidthPriority.High,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SetFilePriorityWithoutIndices_Throws()
    {
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Settings);

        var handler = new ExecuteTorrentActionHandler(store.Object, new Mock<ITransmissionClient>().Object);
        var dto = new TorrentActionDto("set-file-priority", [7], Priority: "normal");

        await Assert.ThrowsAsync<SettingsValidationException>(() => handler.HandleAsync(dto));
    }
}
