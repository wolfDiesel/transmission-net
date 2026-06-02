namespace TransmissonNET.Application.Contracts;

public sealed record TorrentMetainfoPreviewDto(
    string Name,
    string FileName,
    long TotalSize,
    IReadOnlyList<TorrentFileNodeDto> FileTree);

public sealed record TorrentMetainfoInspectRequestDto(string MetainfoBase64);

public sealed record TorrentAddRequestDto(
    string MetainfoBase64,
    string? DownloadDir,
    bool Paused = false);

public sealed record TorrentAddResultDto(int Id, string Name, string HashString);
