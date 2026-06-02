using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Application.Torrents;

namespace TransmissonNET.Application.Handlers;

public sealed class AddTorrentHandler(
    ISettingsStore settingsStore,
    ITransmissionClient transmissionClient)
{
    public async Task<TorrentAddResultDto> HandleAsync(
        TorrentAddRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var bytes = TorrentMetainfoBytes.FromBase64(request.MetainfoBase64);
        _ = TorrentMetainfoParser.Parse(bytes);

        var downloadDir = request.DownloadDir?.Trim();
        if (downloadDir is { Length: 0 })
            throw new SettingsValidationException("Download directory cannot be empty.");

        var settings = await settingsStore.LoadAsync(cancellationToken);
        var result = await transmissionClient.AddTorrentAsync(
            settings.Daemon,
            bytes,
            string.IsNullOrWhiteSpace(downloadDir) ? null : downloadDir,
            request.Paused,
            cancellationToken);

        return new TorrentAddResultDto(result.Id, result.Name, result.HashString);
    }
}
