using System.Text.Json;
using TransmissonNET.Domain;

namespace TransmissonNET.Infrastructure.Rpc;

internal static class TorrentDetailsMapper
{
    private static readonly string[] DetailsFields =
    [
        ..TorrentMapper.Fields,
        "error", "errorString",
        "comment", "creator", "dateCreated",
        "hashString", "pieceSize", "isPrivate",
        "files", "fileStats",
    ];

    public static string[] Fields => DetailsFields;

    public static IReadOnlyList<TorrentDetails> MapTorrents(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("torrents", out var torrents))
            return Array.Empty<TorrentDetails>();

        var list = new List<TorrentDetails>();

        if (torrents.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in torrents.EnumerateArray())
                list.Add(MapTorrentObject(item));
        }

        return list;
    }

    private static TorrentDetails MapTorrentObject(JsonElement item)
    {
        var baseTorrent = TorrentMapper.MapTorrent(item);
        return new TorrentDetails
        {
            Id = baseTorrent.Id,
            Name = baseTorrent.Name,
            Status = baseTorrent.Status,
            PercentDone = baseTorrent.PercentDone,
            RateDownload = baseTorrent.RateDownload,
            RateUpload = baseTorrent.RateUpload,
            Eta = baseTorrent.Eta,
            TotalSize = baseTorrent.TotalSize,
            AddedDate = baseTorrent.AddedDate,
            DoneDate = baseTorrent.DoneDate,
            StartDate = baseTorrent.StartDate,
            UploadRatio = baseTorrent.UploadRatio,
            PeersConnected = baseTorrent.PeersConnected,
            LeftUntilDone = baseTorrent.LeftUntilDone,
            DownloadedEver = baseTorrent.DownloadedEver,
            UploadedEver = baseTorrent.UploadedEver,
            QueuePosition = baseTorrent.QueuePosition,
            DownloadDir = baseTorrent.DownloadDir,
            BandwidthPriority = baseTorrent.BandwidthPriority,
            Error = RpcJsonReader.GetInt32(item, "error"),
            ErrorString = RpcJsonReader.GetString(item, "errorString", "error_string"),
            Comment = RpcJsonReader.GetString(item, "comment"),
            Creator = RpcJsonReader.GetString(item, "creator"),
            DateCreated = RpcJsonReader.GetInt64(item, "dateCreated", "date_created"),
            HashString = RpcJsonReader.GetString(item, "hashString", "hash_string"),
            PieceSize = RpcJsonReader.GetInt64(item, "pieceSize", "piece_size"),
            IsPrivate = RpcJsonReader.GetBoolean(item, "isPrivate", "is_private"),
            Files = MapFiles(item),
        };
    }

    private static IReadOnlyList<TorrentFile> MapFiles(JsonElement item)
    {
        if (!RpcJsonReader.TryGetProperty(item, "files", null, out var filesElement)
            || filesElement.ValueKind != JsonValueKind.Array)
            return Array.Empty<TorrentFile>();

        var hasStats = RpcJsonReader.TryGetProperty(item, "fileStats", "file_stats", out var statsElement)
            && statsElement.ValueKind == JsonValueKind.Array;

        var list = new List<TorrentFile>();
        var index = 0;

        foreach (var fileElement in filesElement.EnumerateArray())
        {
            var stats = hasStats && index < statsElement.GetArrayLength()
                ? statsElement[index]
                : default;

            var bytesCompleted = RpcJsonReader.GetInt64(fileElement, "bytesCompleted", "bytes_completed");
            if (stats.ValueKind != JsonValueKind.Undefined)
            {
                var fromStats = RpcJsonReader.GetInt64(stats, "bytesCompleted", "bytes_completed");
                if (fromStats > 0 || bytesCompleted == 0)
                    bytesCompleted = fromStats;
            }

            list.Add(new TorrentFile
            {
                Index = index,
                Name = RpcJsonReader.GetString(fileElement, "name"),
                Length = RpcJsonReader.GetInt64(fileElement, "length"),
                BytesCompleted = bytesCompleted,
                Wanted = stats.ValueKind == JsonValueKind.Undefined
                    || RpcJsonReader.GetBoolean(stats, "wanted"),
                Priority = stats.ValueKind != JsonValueKind.Undefined
                    ? RpcJsonReader.GetInt32(stats, "priority")
                    : 0,
            });

            index++;
        }

        return list;
    }
}
