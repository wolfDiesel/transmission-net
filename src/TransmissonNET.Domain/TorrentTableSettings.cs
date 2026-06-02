namespace TransmissonNET.Domain;

public sealed record TorrentTableColumnSetting(string Id, bool Visible, int? WidthPx = null);

public sealed record TorrentTableSettings(
    IReadOnlyList<TorrentTableColumnSetting> Columns,
    string SortColumnId,
    bool SortDescending)
{
    public static TorrentTableSettings CreateDefault() =>
        new(
            [
                new(TorrentTableColumnIds.Name, true),
                new(TorrentTableColumnIds.Progress, true),
                new(TorrentTableColumnIds.AddedDate, true),
                new(TorrentTableColumnIds.Status, true),
                new(TorrentTableColumnIds.Size, true),
                new(TorrentTableColumnIds.DownloadSpeed, true),
                new(TorrentTableColumnIds.UploadSpeed, true),
                new(TorrentTableColumnIds.Eta, true),
                new(TorrentTableColumnIds.DoneDate, false),
                new(TorrentTableColumnIds.UploadRatio, false),
                new(TorrentTableColumnIds.Peers, false),
                new(TorrentTableColumnIds.Downloaded, false),
                new(TorrentTableColumnIds.Uploaded, false),
                new(TorrentTableColumnIds.Queue, false),
                new(TorrentTableColumnIds.DownloadDir, false),
                new(TorrentTableColumnIds.Left, false),
            ],
            TorrentTableColumnIds.Name,
            false);
}

public static class TorrentTableColumnIds
{
    public const string Name = "name";
    public const string Progress = "progress";
    public const string AddedDate = "addedDate";
    public const string DoneDate = "doneDate";
    public const string Status = "status";
    public const string Size = "size";
    public const string DownloadSpeed = "downloadSpeed";
    public const string UploadSpeed = "uploadSpeed";
    public const string Eta = "eta";
    public const string UploadRatio = "uploadRatio";
    public const string Peers = "peers";
    public const string Downloaded = "downloaded";
    public const string Uploaded = "uploaded";
    public const string Queue = "queue";
    public const string DownloadDir = "downloadDir";
    public const string Left = "left";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(
        StringComparer.Ordinal)
    {
        Name,
        Progress,
        AddedDate,
        DoneDate,
        Status,
        Size,
        DownloadSpeed,
        UploadSpeed,
        Eta,
        UploadRatio,
        Peers,
        Downloaded,
        Uploaded,
        Queue,
        DownloadDir,
        Left,
    };
}
