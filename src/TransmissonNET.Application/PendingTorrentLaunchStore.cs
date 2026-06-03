using TransmissonNET.Application.Abstractions;

namespace TransmissonNET.Application;

public sealed class PendingTorrentLaunchStore : IPendingTorrentLaunchStore
{
    private readonly object _sync = new();
    private string? _path;

    public void SetPendingPath(string filePath)
    {
        lock (_sync)
            _path = filePath;
    }

    public string? PeekPendingPath()
    {
        lock (_sync)
            return _path;
    }

    public string? TakePendingPath()
    {
        lock (_sync)
        {
            var path = _path;
            _path = null;
            return path;
        }
    }
}
