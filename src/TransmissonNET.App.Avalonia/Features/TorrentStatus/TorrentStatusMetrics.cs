using TransmissonNET.Application.Contracts;

namespace TransmissonNET.App.Avalonia.Features.TorrentStatus;

internal static class TorrentStatusMetrics
{
    public static (long DownloadSpeed, long UploadSpeed, int Downloading, int Completed) Derive(
        IEnumerable<TorrentDto> torrents)
    {
        long downloadSpeed = 0;
        long uploadSpeed = 0;
        var downloading = 0;
        var completed = 0;

        foreach (var torrent in torrents)
        {
            downloadSpeed += torrent.RateDownload;
            uploadSpeed += torrent.RateUpload;

            if (torrent.Status is Domain.TorrentStatus.Downloading or Domain.TorrentStatus.DownloadWait)
                downloading++;

            if (torrent.PercentDone >= 1 || torrent.Status == Domain.TorrentStatus.Seeding)
                completed++;
        }

        return (downloadSpeed, uploadSpeed, downloading, completed);
    }
}
