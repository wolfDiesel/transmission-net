using Moq;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Tests.Handlers;

public class GetTorrentFileAssociationStatusHandlerTests
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
    public async Task HandleAsync_WhenNotAskedAndNotDefault_ShouldPrompt()
    {
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultSettings(TorrentFileAssociationStatuses.NotAsked));

        var association = new Mock<ITorrentFileAssociationService>();
        association.Setup(a => a.IsSupported).Returns(true);
        association.Setup(a => a.HasDesktopEntry()).Returns(false);
        association.Setup(a => a.IsDefaultHandler()).Returns(false);

        var handler = new GetTorrentFileAssociationStatusHandler(association.Object, store.Object);
        var result = await handler.HandleAsync();

        Assert.True(result.ShouldPrompt);
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyDefault_ShouldNotPrompt()
    {
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultSettings(TorrentFileAssociationStatuses.NotAsked));

        var association = new Mock<ITorrentFileAssociationService>();
        association.Setup(a => a.IsSupported).Returns(true);
        association.Setup(a => a.HasDesktopEntry()).Returns(true);
        association.Setup(a => a.IsDefaultHandler()).Returns(true);

        var handler = new GetTorrentFileAssociationStatusHandler(association.Object, store.Object);
        var result = await handler.HandleAsync();

        Assert.False(result.ShouldPrompt);
    }

    [Fact]
    public async Task HandleAsync_WhenDeclined_ShouldNotPrompt()
    {
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultSettings(TorrentFileAssociationStatuses.Declined));

        var association = new Mock<ITorrentFileAssociationService>();
        association.Setup(a => a.IsSupported).Returns(true);
        association.Setup(a => a.HasDesktopEntry()).Returns(false);
        association.Setup(a => a.IsDefaultHandler()).Returns(false);

        var handler = new GetTorrentFileAssociationStatusHandler(association.Object, store.Object);
        var result = await handler.HandleAsync();

        Assert.False(result.ShouldPrompt);
    }
}
