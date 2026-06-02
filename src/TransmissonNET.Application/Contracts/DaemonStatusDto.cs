namespace TransmissonNET.Application.Contracts;

public sealed record DaemonStatusDto(
    bool Connected,
    long DownloadSpeed,
    long UploadSpeed,
    int DownloadingCount,
    int CompletedCount);
