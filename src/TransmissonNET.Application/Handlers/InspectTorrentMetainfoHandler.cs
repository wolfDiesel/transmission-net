using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Torrents;

namespace TransmissonNET.Application.Handlers;

public sealed class InspectTorrentMetainfoHandler
{
    public Task<TorrentMetainfoPreviewDto> HandleAsync(
        TorrentMetainfoInspectRequestDto request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bytes = TorrentMetainfoBytes.FromBase64(request.MetainfoBase64);
        return Task.FromResult(TorrentMetainfoParser.Parse(bytes));
    }
}
