using TransmissonNET.Application.Abstractions;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Handlers;

public sealed class GetTorrentsHandler(
    ISettingsStore settingsStore,
    ITransmissionClient transmissionClient)
{
    public async Task<IReadOnlyList<Torrent>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        return await transmissionClient.GetTorrentsAsync(settings.Daemon, cancellationToken);
    }
}
