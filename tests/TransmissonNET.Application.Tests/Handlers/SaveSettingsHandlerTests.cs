using Moq;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Tests.Handlers;

public class SaveSettingsHandlerTests
{
    private static readonly AppSettings Existing = new(
        new DaemonConnection("127.0.0.1", 9091, "/transmission/rpc", "user", "secret"),
        new UiSettings(3, 1280, 800, TorrentTableSettings.CreateDefault(), UiColorSchemes.Default, UiAppearances.Default));

    [Theory]
    [InlineData(0)]
    [InlineData(70000)]
    public async Task HandleAsync_InvalidPort_Throws(int port)
    {
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Existing);

        var handler = new SaveSettingsHandler(store.Object);
        var dto = new AppSettingsDto(
            new DaemonConnectionDto("127.0.0.1", port, "/transmission/rpc", "user", null),
            new UiSettingsDto(3, 1280, 800, TestTorrentTableSettingsDto.Default));

        await Assert.ThrowsAsync<SettingsValidationException>(() => handler.HandleAsync(dto));
    }
}
