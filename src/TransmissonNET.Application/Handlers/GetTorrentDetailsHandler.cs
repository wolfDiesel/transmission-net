using TransmissonNET.Application.Abstractions;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Handlers;

public sealed class GetTorrentDetailsHandler(
    ISettingsStore settingsStore,
    ITransmissionClient transmissionClient)
{
    public async Task<TorrentDetails?> HandleAsync(int id, CancellationToken cancellationToken = default)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        return await transmissionClient.GetTorrentDetailsAsync(settings.Daemon, id, cancellationToken);
    }
}
