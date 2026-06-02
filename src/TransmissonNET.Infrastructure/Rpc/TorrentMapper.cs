using System.Text.Json;
using TransmissonNET.Domain;

namespace TransmissonNET.Infrastructure.Rpc;

internal static class TorrentMapper
{
    private static readonly string[] TorrentFields =
    [
        "id", "name", "status", "percentDone", "rateDownload", "rateUpload",
        "eta", "totalSize", "addedDate", "doneDate", "startDate", "uploadRatio",
        "peersConnected", "leftUntilDone", "downloadedEver", "uploadedEver",
        "queuePosition", "downloadDir", "bandwidthPriority"
    ];

    public static IReadOnlyList<Torrent> MapTorrents(JsonElement arguments) =>
        MapTorrents(arguments, MapTorrentObject);

    public static Torrent MapTorrent(JsonElement item) => MapTorrentObject(item);

    public static string[] Fields => TorrentFields;

    private static IReadOnlyList<TTorrent> MapTorrents<TTorrent>(
        JsonElement arguments,
        Func<JsonElement, TTorrent> map)
    {
        if (!arguments.TryGetProperty("torrents", out var torrents))
            return Array.Empty<TTorrent>();

        var list = new List<TTorrent>();

        if (torrents.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in torrents.EnumerateArray())
                list.Add(map(item));
        }

        return list;
    }

    private static Torrent MapTorrentObject(JsonElement item)
    {
        var statusCode = item.GetProperty("status").GetInt32();
        return new Torrent
        {
            Id = item.GetProperty("id").GetInt32(),
            Name = item.GetProperty("name").GetString() ?? string.Empty,
            Status = Enum.IsDefined(typeof(TorrentStatus), statusCode)
                ? (TorrentStatus)statusCode
                : TorrentStatus.Unknown,
            PercentDone = RpcJsonReader.GetDouble(item, "percentDone"),
            RateDownload = RpcJsonReader.GetInt64(item, "rateDownload"),
            RateUpload = RpcJsonReader.GetInt64(item, "rateUpload"),
            Eta = RpcJsonReader.GetInt64(item, "eta"),
            TotalSize = RpcJsonReader.GetInt64(item, "totalSize"),
            AddedDate = RpcJsonReader.GetInt64(item, "addedDate"),
            DoneDate = RpcJsonReader.GetInt64(item, "doneDate"),
            StartDate = RpcJsonReader.GetInt64(item, "startDate"),
            UploadRatio = RpcJsonReader.GetDouble(item, "uploadRatio"),
            PeersConnected = RpcJsonReader.GetInt32(item, "peersConnected"),
            LeftUntilDone = RpcJsonReader.GetInt64(item, "leftUntilDone"),
            DownloadedEver = RpcJsonReader.GetInt64(item, "downloadedEver"),
            UploadedEver = RpcJsonReader.GetInt64(item, "uploadedEver"),
            QueuePosition = RpcJsonReader.GetInt32(item, "queuePosition"),
            DownloadDir = item.TryGetProperty("downloadDir", out var dir)
                ? dir.GetString() ?? string.Empty
                : string.Empty,
            BandwidthPriority = ParseBandwidthPriority(item),
        };
    }

    private static TorrentBandwidthPriority ParseBandwidthPriority(JsonElement item)
    {
        if (item.TryGetProperty("bandwidthPriority", out var camel))
            return ToPriority(camel.GetInt32());

        if (item.TryGetProperty("bandwidth_priority", out var snake))
            return ToPriority(snake.GetInt32());

        return TorrentBandwidthPriority.Normal;
    }

    private static TorrentBandwidthPriority ToPriority(int value) =>
        value switch
        {
            < 0 => TorrentBandwidthPriority.Low,
            > 0 => TorrentBandwidthPriority.High,
            _ => TorrentBandwidthPriority.Normal,
        };

}
