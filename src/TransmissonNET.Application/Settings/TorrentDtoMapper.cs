using TransmissonNET.Application.Contracts;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Settings;

public static class TorrentDtoMapper
{
    public static TorrentDto ToDto(Torrent torrent) =>
        new(
            torrent.Id,
            torrent.Name,
            torrent.Status,
            torrent.PercentDone,
            torrent.RateDownload,
            torrent.RateUpload,
            torrent.Eta,
            torrent.TotalSize,
            torrent.AddedDate,
            torrent.DoneDate,
            torrent.StartDate,
            torrent.UploadRatio,
            torrent.PeersConnected,
            torrent.LeftUntilDone,
            torrent.DownloadedEver,
            torrent.UploadedEver,
            torrent.QueuePosition,
            torrent.DownloadDir,
            PriorityToString(torrent.BandwidthPriority));

    public static IReadOnlyList<TorrentDto> ToDtoList(IEnumerable<Torrent> torrents) =>
        torrents.Select(ToDto).ToList();

    private static string PriorityToString(TorrentBandwidthPriority priority) =>
        priority switch
        {
            TorrentBandwidthPriority.Low => "low",
            TorrentBandwidthPriority.High => "high",
            _ => "normal",
        };
}
