using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Handlers;

public sealed class GetDaemonStatusHandler(
    ISettingsStore settingsStore,
    ITransmissionClient transmissionClient)
{
    public async Task<DaemonStatusDto> HandleAsync(
        bool includeCounts = true,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);

        try
        {
            var session = await transmissionClient.GetSessionAsync(settings.Daemon, cancellationToken);
            var counts = includeCounts
                ? await transmissionClient.GetTorrentStatusCountsAsync(settings.Daemon, cancellationToken)
                : new TorrentStatusCounts(0, 0);

            return new DaemonStatusDto(
                true,
                session.DownloadSpeed,
                session.UploadSpeed,
                counts.Downloading,
                counts.Completed);
        }
        catch (DaemonConnectionException)
        {
            return new DaemonStatusDto(false, 0, 0, 0, 0);
        }
    }
}
