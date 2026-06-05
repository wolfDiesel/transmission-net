namespace TransmissonNET.App.Avalonia.Features.TorrentTable;

internal static class TorrentTableColumns
{
    internal sealed record ColumnDef(string Id, string BindingPath, bool Sortable, int DefaultWidthPx);

    private static readonly ColumnDef[] Ordered =
    [
        new(TransmissonNET.Domain.TorrentTableColumnIds.Name, nameof(ViewModels.TorrentRowViewModel.Name), true, 220),
        new(TransmissonNET.Domain.TorrentTableColumnIds.Progress, nameof(ViewModels.TorrentRowViewModel.PercentDone), true, 140),
        new(TransmissonNET.Domain.TorrentTableColumnIds.AddedDate, nameof(ViewModels.TorrentRowViewModel.AddedDate), true, 150),
        new(TransmissonNET.Domain.TorrentTableColumnIds.DoneDate, nameof(ViewModels.TorrentRowViewModel.DoneDate), true, 150),
        new(TransmissonNET.Domain.TorrentTableColumnIds.Status, nameof(ViewModels.TorrentRowViewModel.Status), true, 120),
        new(TransmissonNET.Domain.TorrentTableColumnIds.Size, nameof(ViewModels.TorrentRowViewModel.Size), true, 90),
        new(TransmissonNET.Domain.TorrentTableColumnIds.DownloadSpeed, nameof(ViewModels.TorrentRowViewModel.DownloadSpeed), true, 90),
        new(TransmissonNET.Domain.TorrentTableColumnIds.UploadSpeed, nameof(ViewModels.TorrentRowViewModel.UploadSpeed), true, 90),
        new(TransmissonNET.Domain.TorrentTableColumnIds.Eta, nameof(ViewModels.TorrentRowViewModel.Eta), true, 80),
        new(TransmissonNET.Domain.TorrentTableColumnIds.UploadRatio, nameof(ViewModels.TorrentRowViewModel.UploadRatio), true, 80),
        new(TransmissonNET.Domain.TorrentTableColumnIds.Peers, nameof(ViewModels.TorrentRowViewModel.Peers), true, 80),
        new(TransmissonNET.Domain.TorrentTableColumnIds.Downloaded, nameof(ViewModels.TorrentRowViewModel.Downloaded), true, 100),
        new(TransmissonNET.Domain.TorrentTableColumnIds.Uploaded, nameof(ViewModels.TorrentRowViewModel.Uploaded), true, 100),
        new(TransmissonNET.Domain.TorrentTableColumnIds.Queue, nameof(ViewModels.TorrentRowViewModel.Queue), true, 80),
        new(TransmissonNET.Domain.TorrentTableColumnIds.DownloadDir, nameof(ViewModels.TorrentRowViewModel.DownloadDir), false, 180),
        new(TransmissonNET.Domain.TorrentTableColumnIds.Left, nameof(ViewModels.TorrentRowViewModel.Left), true, 90),
    ];

    public static IReadOnlyList<ColumnDef> All => Ordered;

    public static ColumnDef? Find(string id) =>
        Ordered.FirstOrDefault(column => column.Id == id);
}
