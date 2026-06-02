namespace TransmissonNET.Application.Contracts;

public sealed record TorrentFileNodeDto(
    string Name,
    string Path,
    bool IsFolder,
    int? FileIndex,
    long Length,
    long BytesCompleted,
    bool? Wanted,
    int? Priority,
    IReadOnlyList<TorrentFileNodeDto> Children);
