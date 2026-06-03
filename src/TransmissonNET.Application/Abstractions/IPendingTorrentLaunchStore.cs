namespace TransmissonNET.Application.Abstractions;

public interface IPendingTorrentLaunchStore
{
    void SetPendingPath(string filePath);

    string? TakePendingPath();
}
