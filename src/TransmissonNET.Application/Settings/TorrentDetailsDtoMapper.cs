using TransmissonNET.Application.Contracts;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Settings;

public static class TorrentDetailsDtoMapper
{
    public static TorrentDetailsDto ToDto(TorrentDetails details) =>
        new(
            details.Id,
            details.Name,
            details.Status,
            details.PercentDone,
            details.RateDownload,
            details.RateUpload,
            details.Eta,
            details.TotalSize,
            details.AddedDate,
            details.DoneDate,
            details.StartDate,
            details.UploadRatio,
            details.PeersConnected,
            details.LeftUntilDone,
            details.DownloadedEver,
            details.UploadedEver,
            details.QueuePosition,
            details.DownloadDir,
            PriorityToString(details.BandwidthPriority),
            details.Error,
            details.ErrorString,
            details.Comment,
            details.Creator,
            details.DateCreated,
            details.HashString,
            details.PieceSize,
            details.IsPrivate,
            TorrentFileTreeBuilder.Build(details.Files));

    private static string PriorityToString(TorrentBandwidthPriority priority) =>
        priority switch
        {
            TorrentBandwidthPriority.Low => "low",
            TorrentBandwidthPriority.High => "high",
            _ => "normal",
        };
}
