namespace TransmissonNET.Domain;

public sealed class Torrent
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TorrentStatus Status { get; init; }
    public double PercentDone { get; init; }
    public long RateDownload { get; init; }
    public long RateUpload { get; init; }
    public long Eta { get; init; }
    public long TotalSize { get; init; }
    public long AddedDate { get; init; }
    public long DoneDate { get; init; }
    public long StartDate { get; init; }
    public double UploadRatio { get; init; }
    public int PeersConnected { get; init; }
    public long LeftUntilDone { get; init; }
    public long DownloadedEver { get; init; }
    public long UploadedEver { get; init; }
    public int QueuePosition { get; init; }
    public string DownloadDir { get; init; } = string.Empty;
    public TorrentBandwidthPriority BandwidthPriority { get; init; } = TorrentBandwidthPriority.Normal;
}
