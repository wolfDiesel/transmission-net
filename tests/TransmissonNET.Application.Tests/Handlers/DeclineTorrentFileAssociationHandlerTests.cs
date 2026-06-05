using Moq;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Tests.Handlers;

public class DeclineTorrentFileAssociationHandlerTests
{
    [Fact]
    public async Task HandleAsync_SavesDeclinedStatus()
    {
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppSettings(
                new DaemonConnection("127.0.0.1", 9091, "/transmission/rpc", "", ""),
                new UiSettings(
                    3,
                    1280,
                    800,
                    TorrentTableSettings.CreateDefault(),
                    TorrentFileAssociation: TorrentFileAssociationStatuses.NotAsked)));
        AppSettings? saved = null;
        store.Setup(s => s.SaveAsync(It.IsAny<AppSettings>(), It.IsAny<CancellationToken>()))
            .Callback<AppSettings, CancellationToken>((settings, _) => saved = settings)
            .Returns(Task.CompletedTask);

        var handler = new DeclineTorrentFileAssociationHandler(store.Object);
        await handler.HandleAsync();

        Assert.Equal(TorrentFileAssociationStatuses.Declined, saved?.Ui.TorrentFileAssociation);
    }
}
