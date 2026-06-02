using TransmissonNET.Domain;

namespace TransmissonNET.Application.Contracts;

public sealed record TorrentDetailsDto(
    int Id,
    string Name,
    TorrentStatus Status,
    double PercentDone,
    long RateDownload,
    long RateUpload,
    long Eta,
    long TotalSize,
    long AddedDate,
    long DoneDate,
    long StartDate,
    double UploadRatio,
    int PeersConnected,
    long LeftUntilDone,
    long DownloadedEver,
    long UploadedEver,
    int QueuePosition,
    string DownloadDir,
    string BandwidthPriority,
    int Error,
    string ErrorString,
    string Comment,
    string Creator,
    long DateCreated,
    string HashString,
    long PieceSize,
    bool IsPrivate,
    IReadOnlyList<TorrentFileNodeDto> FileTree);
