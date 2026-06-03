using TransmissonNET.Application.Abstractions;

namespace TransmissonNET.Application.Handlers;

public sealed class GetPendingTorrentLaunchPathHandler(IPendingTorrentLaunchStore pendingStore)
{
    public Task<string?> HandleAsync(bool consume = true, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(consume ? pendingStore.TakePendingPath() : pendingStore.PeekPendingPath());
    }
}
