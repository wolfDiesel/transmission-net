using Moq;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Tests.Handlers;

public class RegisterTorrentFileAssociationHandlerTests
{
    private static AppSettings DefaultSettings(string associationStatus) =>
        new(
            new DaemonConnection("127.0.0.1", 9091, "/transmission/rpc", "", ""),
            new UiSettings(
                3,
                1280,
                800,
                TorrentTableSettings.CreateDefault(),
                TorrentFileAssociation: associationStatus));

    [Fact]
    public async Task HandleAsync_WhenSupported_RegistersAndSavesStatus()
    {
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultSettings(TorrentFileAssociationStatuses.NotAsked));
        AppSettings? saved = null;
        store.Setup(s => s.SaveAsync(It.IsAny<AppSettings>(), It.IsAny<CancellationToken>()))
            .Callback<AppSettings, CancellationToken>((settings, _) => saved = settings)
            .Returns(Task.CompletedTask);

        var association = new Mock<ITorrentFileAssociationService>();
        association.Setup(a => a.IsSupported).Returns(true);

        var handler = new RegisterTorrentFileAssociationHandler(association.Object, store.Object);
        await handler.HandleAsync();

        association.Verify(a => a.RegisterAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(TorrentFileAssociationStatuses.Registered, saved?.Ui.TorrentFileAssociation);
    }

    [Fact]
    public async Task HandleAsync_WhenUnsupported_ThrowsWithoutRegistering()
    {
        var store = new Mock<ISettingsStore>();
        var association = new Mock<ITorrentFileAssociationService>();
        association.Setup(a => a.IsSupported).Returns(false);

        var handler = new RegisterTorrentFileAssociationHandler(association.Object, store.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync());
        association.Verify(a => a.RegisterAsync(It.IsAny<CancellationToken>()), Times.Never);
        store.Verify(s => s.SaveAsync(It.IsAny<AppSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
