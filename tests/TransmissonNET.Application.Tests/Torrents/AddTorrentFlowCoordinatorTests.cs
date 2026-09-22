using Moq;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Torrents;

namespace TransmissonNET.Application.Tests.Torrents;

public class AddTorrentFlowCoordinatorTests
{
    private readonly Mock<IAddTorrentService> _addTorrentService = new();
    private readonly Mock<IAddTorrentCompletionActions> _completionActions = new();

    private AddTorrentFlowCoordinator CreateCoordinator() =>
        new(_addTorrentService.Object, _completionActions.Object);

    [Fact]
    public async Task AddAndFocusAsync_OnSuccess_TrimsDirectoryAndRunsCompletionActionsInOrder()
    {
        var result = new TorrentAddResultDto(42, "ubuntu.iso", "abc123");
        var order = new List<string>();

        _addTorrentService
            .Setup(s => s.AddAsync(It.IsAny<TorrentAddRequestDto>(), It.IsAny<CancellationToken>()))
            .Callback(() => order.Add("add"))
            .ReturnsAsync(result);

        _completionActions
            .Setup(a => a.RememberDownloadDirectory(It.IsAny<string>()))
            .Callback<string>(_ => order.Add("remember"));
        _completionActions
            .Setup(a => a.SwitchToTorrentList())
            .Callback(() => order.Add("switch"));
        _completionActions
            .Setup(a => a.FocusTorrentAsync(It.IsAny<int>()))
            .Callback<int>(_ => order.Add("focus"));

        var returned = await CreateCoordinator().AddAndFocusAsync("b64", "  /downloads  ", false);

        _addTorrentService.Verify(
            s => s.AddAsync(
                It.Is<TorrentAddRequestDto>(r =>
                    r.MetainfoBase64 == "b64" &&
                    r.DownloadDir == "/downloads" &&
                    !r.Paused),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _completionActions.Verify(a => a.RememberDownloadDirectory("/downloads"), Times.Once);
        _completionActions.Verify(a => a.SwitchToTorrentList(), Times.Once);
        _completionActions.Verify(a => a.FocusTorrentAsync(42), Times.Once);

        Assert.Equal(result, returned);
        Assert.Equal(new[] { "add", "remember", "switch", "focus" }, order);
    }

    [Fact]
    public async Task AddAndFocusAsync_PassesPausedFlagThrough()
    {
        var result = new TorrentAddResultDto(7, "name", "hash");
        _addTorrentService
            .Setup(s => s.AddAsync(It.IsAny<TorrentAddRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        await CreateCoordinator().AddAndFocusAsync("b64", "/dl", true);

        _addTorrentService.Verify(
            s => s.AddAsync(
                It.Is<TorrentAddRequestDto>(r => r.Paused),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddAndFocusAsync_WhenAddFails_DoesNotRunCompletionActionsAndPropagates()
    {
        var expected = new InvalidOperationException("boom");
        _addTorrentService
            .Setup(s => s.AddAsync(It.IsAny<TorrentAddRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateCoordinator().AddAndFocusAsync("b64", "/dl", true));

        Assert.Same(expected, exception);
        _completionActions.Verify(a => a.RememberDownloadDirectory(It.IsAny<string>()), Times.Never);
        _completionActions.Verify(a => a.SwitchToTorrentList(), Times.Never);
        _completionActions.Verify(a => a.FocusTorrentAsync(It.IsAny<int>()), Times.Never);
    }
}
