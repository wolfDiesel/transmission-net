using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;

namespace TransmissonNET.Application.Torrents;

public sealed class AddTorrentFlowCoordinator(
    IAddTorrentService addTorrentService,
    IAddTorrentCompletionActions completionActions)
{
    public async Task<TorrentAddResultDto> AddAndFocusAsync(
        string metainfoBase64,
        string downloadDir,
        bool addPaused,
        CancellationToken cancellationToken = default)
    {
        var destination = downloadDir.Trim();

        var result = await addTorrentService.AddAsync(
            new TorrentAddRequestDto(metainfoBase64, destination, addPaused),
            cancellationToken);

        completionActions.RememberDownloadDirectory(destination);
        completionActions.SwitchToTorrentList();
        await completionActions.FocusTorrentAsync(result.Id);

        return result;
    }
}
