using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Handlers;

public sealed class GetTorrentFileAssociationStatusHandler(
    ITorrentFileAssociationService associationService,
    ISettingsStore settingsStore)
{
    public async Task<TorrentFileAssociationStatusDto> HandleAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        var status = TorrentFileAssociationStatuses.Normalize(settings.Ui.TorrentFileAssociation);
        var hasDesktopEntry = associationService.IsSupported && associationService.HasDesktopEntry();
        var isDefault = associationService.IsSupported && associationService.IsDefaultHandler();
        var shouldPrompt = associationService.IsSupported
            && status == TorrentFileAssociationStatuses.NotAsked
            && !isDefault;

        return new TorrentFileAssociationStatusDto(
            associationService.IsSupported,
            hasDesktopEntry,
            isDefault,
            status,
            shouldPrompt);
    }
}
