using TransmissonNET.Application.Abstractions;

namespace TransmissonNET.Application;

public sealed class PendingTorrentLaunchStore : IPendingTorrentLaunchStore
{
    private string? _path;

    public void SetPendingPath(string filePath) => _path = filePath;

    public string? TakePendingPath()
    {
        var path = _path;
        _path = null;
        return path;
    }
}
