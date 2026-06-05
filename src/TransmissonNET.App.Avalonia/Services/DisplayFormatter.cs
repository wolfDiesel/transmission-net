using TransmissonNET.Domain;

namespace TransmissonNET.App.Avalonia.Services;

internal static class DisplayFormatter
{
    public static string Bytes(long bytes)
    {
        if (bytes <= 0)
            return "—";

        var units = new[] { "B", "KB", "MB", "GB", "TB" };
        var value = (double)bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.#} {units[unit]}";
    }

    public static string Speed(long bytesPerSecond) =>
        bytesPerSecond <= 0 ? "—" : $"{Bytes(bytesPerSecond)}/s";

    public static string Eta(long seconds) =>
        seconds < 0 ? "—" : seconds switch
        {
            < 60 => $"{seconds}s",
            < 3600 => $"{seconds / 60}m",
            _ => $"{seconds / 3600}h {(seconds % 3600) / 60}m",
        };

    public static string Percent(double value) => $"{value * 100:0.0}%";

    public static string UnixDate(long unixSeconds) =>
        unixSeconds <= 0
            ? "—"
            : DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime.ToString("g");

    public static string Status(TorrentStatus status) =>
        status switch
        {
            TorrentStatus.Stopped => "Stopped",
            TorrentStatus.CheckWait => "Queued (check)",
            TorrentStatus.Checking => "Checking",
            TorrentStatus.DownloadWait => "Queued",
            TorrentStatus.Downloading => "Downloading",
            TorrentStatus.SeedWait => "Queued (seed)",
            TorrentStatus.Seeding => "Seeding",
            _ => "Unknown",
        };
}
