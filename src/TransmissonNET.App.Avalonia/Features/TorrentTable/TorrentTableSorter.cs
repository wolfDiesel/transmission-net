using System.Collections.ObjectModel;
using TransmissonNET.Application.Contracts;
using TransmissonNET.App.Avalonia.ViewModels;
using TransmissonNET.Domain;

namespace TransmissonNET.App.Avalonia.Features.TorrentTable;

internal static class TorrentTableSorter
{
    public static IReadOnlyList<TorrentRowViewModel> SortOrdered(
        IEnumerable<TorrentRowViewModel> rows,
        string columnId,
        bool descending)
    {
        var ordered = rows
            .OrderBy(row => GetSortValue(row.Source, columnId), SortValueComparer.Instance)
            .ToList();

        if (descending)
            ordered.Reverse();

        return ordered;
    }

    public static void SortInPlace(ObservableCollection<TorrentRowViewModel> rows, string columnId, bool descending)
    {
        if (rows.Count < 2)
            return;

        var ordered = SortOrdered(rows, columnId, descending);
        if (IsSameRowOrder(rows, ordered))
            return;

        rows.Clear();
        foreach (var row in ordered)
            rows.Add(row);
    }

    public static bool IsSameRowOrder(
        IReadOnlyList<TorrentRowViewModel> current,
        IReadOnlyList<TorrentRowViewModel> ordered) =>
        IsSameOrder(current, ordered);

    public static void SyncRowOrder(
        ObservableCollection<TorrentRowViewModel> target,
        IReadOnlyList<TorrentRowViewModel> desired)
    {
        if (IsSameRowOrder(target, desired))
            return;

        var desiredIds = desired.Select(row => row.Id).ToHashSet();
        for (var index = target.Count - 1; index >= 0; index--)
        {
            if (!desiredIds.Contains(target[index].Id))
                target.RemoveAt(index);
        }

        for (var index = 0; index < desired.Count; index++)
        {
            var row = desired[index];
            var currentIndex = IndexOfRow(target, row.Id);
            if (currentIndex < 0)
                target.Insert(Math.Min(index, target.Count), row);
            else if (currentIndex != index)
                target.Move(currentIndex, index);
        }
    }

    private static int IndexOfRow(IReadOnlyList<TorrentRowViewModel> rows, int id)
    {
        for (var index = 0; index < rows.Count; index++)
        {
            if (rows[index].Id == id)
                return index;
        }

        return -1;
    }

    private static bool IsSameOrder(IReadOnlyList<TorrentRowViewModel> current, IReadOnlyList<TorrentRowViewModel> ordered)
    {
        if (current.Count != ordered.Count)
            return false;

        for (var index = 0; index < current.Count; index++)
        {
            if (current[index].Id != ordered[index].Id)
                return false;
        }

        return true;
    }

    private static object GetSortValue(TorrentDto torrent, string columnId) =>
        columnId switch
        {
            TorrentTableColumnIds.Name => torrent.Name.ToLowerInvariant(),
            TorrentTableColumnIds.Progress => torrent.PercentDone,
            TorrentTableColumnIds.AddedDate => torrent.AddedDate,
            TorrentTableColumnIds.DoneDate => torrent.DoneDate,
            TorrentTableColumnIds.Status => torrent.Status,
            TorrentTableColumnIds.Size => torrent.TotalSize,
            TorrentTableColumnIds.DownloadSpeed => torrent.RateDownload,
            TorrentTableColumnIds.UploadSpeed => torrent.RateUpload,
            TorrentTableColumnIds.Eta => torrent.Eta,
            TorrentTableColumnIds.UploadRatio => torrent.UploadRatio,
            TorrentTableColumnIds.Peers => torrent.PeersConnected,
            TorrentTableColumnIds.Downloaded => torrent.DownloadedEver,
            TorrentTableColumnIds.Uploaded => torrent.UploadedEver,
            TorrentTableColumnIds.Queue => torrent.QueuePosition,
            TorrentTableColumnIds.DownloadDir => torrent.DownloadDir.ToLowerInvariant(),
            TorrentTableColumnIds.Left => torrent.LeftUntilDone,
            _ => string.Empty,
        };

    private sealed class SortValueComparer : IComparer<object>
    {
        public static SortValueComparer Instance { get; } = new();

        public int Compare(object? left, object? right)
        {
            if (left is IComparable comparable && right is IComparable other)
                return comparable.CompareTo(other);

            return string.CompareOrdinal(left?.ToString(), right?.ToString());
        }
    }
}
