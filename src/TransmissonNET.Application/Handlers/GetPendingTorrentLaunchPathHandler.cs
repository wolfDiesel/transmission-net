using TransmissonNET.Application.Abstractions;

namespace TransmissonNET.Application.Handlers;

public sealed class GetPendingTorrentLaunchPathHandler(IPendingTorrentLaunchStore pendingStore)
{
    public Task<string?> HandleAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(pendingStore.TakePendingPath());
    }
}
