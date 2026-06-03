using TransmissonNET.Application.Abstractions;

namespace TransmissonNET.Infrastructure.Desktop;

public sealed class NullTorrentFileAssociationService : ITorrentFileAssociationService
{
    public bool IsSupported => false;

    public bool IsDefaultHandler() => false;

    public bool HasDesktopEntry() => false;

    public Task RegisterAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
