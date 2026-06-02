using System.Text.Json;
using TransmissonNET.Domain;

namespace TransmissonNET.Infrastructure.Rpc;

internal static class TorrentStatusCountsMapper
{
    private static readonly string[] Fields = ["status", "percentDone"];

    public static string[] RpcFields => Fields;

    public static TorrentStatusCounts Map(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("torrents", out var torrents)
            || torrents.ValueKind != JsonValueKind.Array)
            return new TorrentStatusCounts(0, 0);

        var downloading = 0;
        var completed = 0;

        foreach (var item in torrents.EnumerateArray())
        {
            var statusCode = item.GetProperty("status").GetInt32();
            var status = Enum.IsDefined(typeof(TorrentStatus), statusCode)
                ? (TorrentStatus)statusCode
                : TorrentStatus.Unknown;
            var percentDone = RpcJsonReader.GetDouble(item, "percentDone");

            if (IsDownloading(status))
                downloading++;
            if (IsCompleted(status, percentDone))
                completed++;
        }

        return new TorrentStatusCounts(downloading, completed);
    }

    private static bool IsDownloading(TorrentStatus status) =>
        status is TorrentStatus.Downloading or TorrentStatus.DownloadWait;

    private static bool IsCompleted(TorrentStatus status, double percentDone) =>
        percentDone >= 1.0 || status == TorrentStatus.Seeding;
}
