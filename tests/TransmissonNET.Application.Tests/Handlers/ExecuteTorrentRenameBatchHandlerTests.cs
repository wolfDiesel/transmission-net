using Moq;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Tests.Handlers;

public class ExecuteTorrentRenameBatchHandlerTests
{
    [Fact]
    public async Task HandleAsync_AppliesAllOperations()
    {
        var store = new Mock<ISettingsStore>();
        var settings = new AppSettings(
            new DaemonConnection("127.0.0.1", 9091, "/transmission/rpc", "", ""),
            new UiSettings(3, 1280, 800, TorrentTableSettings.CreateDefault(), UiColorSchemes.Default, UiAppearances.Default));
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);

        var client = new Mock<ITransmissionClient>();
        client
            .Setup(c => c.RenameTorrentPathAsync(
                It.IsAny<DaemonConnection>(),
                5,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new ExecuteTorrentRenameBatchHandler(store.Object, client.Object);
        var result = await handler.HandleAsync(
            5,
            new TorrentRenameBatchRequestDto(
            [
                new TorrentRenameOperationDto("a.mkv", "a2.mkv"),
                new TorrentRenameOperationDto("b.mkv", "b2.mkv"),
            ]));

        Assert.Equal(2, result.Applied);
        Assert.Empty(result.Failures);
        client.Verify(
            c => c.RenameTorrentPathAsync(
                It.IsAny<DaemonConnection>(),
                5,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
