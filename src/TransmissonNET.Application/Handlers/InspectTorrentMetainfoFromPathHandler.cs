using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Torrents;

namespace TransmissonNET.Application.Handlers;

public sealed class InspectTorrentMetainfoFromPathHandler
{
    public Task<TorrentMetainfoFromPathDto> HandleAsync(
        TorrentMetainfoInspectPathRequestDto request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = NormalizeTorrentPath(request.FilePath);
        if (!File.Exists(path))
            throw new FileNotFoundException("Torrent file was not found.", path);

        var bytes = File.ReadAllBytes(path);
        var preview = TorrentMetainfoParser.Parse(bytes, Path.GetFileName(path));
        var metainfoBase64 = Convert.ToBase64String(bytes);
        return Task.FromResult(new TorrentMetainfoFromPathDto(metainfoBase64, preview));
    }

    private static string NormalizeTorrentPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Torrent file path is required.", nameof(filePath));

        var fullPath = Path.GetFullPath(filePath);
        if (!fullPath.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only .torrent files are supported.", nameof(filePath));

        return fullPath;
    }
}
