using TransmissonNET.Application.Abstractions;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Handlers;

public sealed class RegisterTorrentFileAssociationHandler(
    ITorrentFileAssociationService associationService,
    ISettingsStore settingsStore)
{
    public async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        if (!associationService.IsSupported)
            throw new InvalidOperationException("Torrent file association is not supported on this platform.");

        await associationService.RegisterAsync(cancellationToken);

        var settings = await settingsStore.LoadAsync(cancellationToken);
        await settingsStore.SaveAsync(
            settings with
            {
                Ui = settings.Ui with { TorrentFileAssociation = TorrentFileAssociationStatuses.Registered },
            },
            cancellationToken);
    }
}
