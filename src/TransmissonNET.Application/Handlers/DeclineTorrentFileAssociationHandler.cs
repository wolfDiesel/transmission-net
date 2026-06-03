using TransmissonNET.Application.Abstractions;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Handlers;

public sealed class DeclineTorrentFileAssociationHandler(ISettingsStore settingsStore)
{
    public async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        await settingsStore.SaveAsync(
            settings with
            {
                Ui = settings.Ui with { TorrentFileAssociation = TorrentFileAssociationStatuses.Declined },
            },
            cancellationToken);
    }
}
