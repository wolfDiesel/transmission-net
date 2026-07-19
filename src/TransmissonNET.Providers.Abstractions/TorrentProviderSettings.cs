namespace TransmissonNET.Providers.Abstractions;

public sealed class TorrentProviderSettings
{
    public int RequestTimeoutSeconds { get; set; } = 10;

    public string? BaseUrl { get; set; }

    public string? PreferredQuality { get; set; }

    public int MaxSeriesExpand { get; set; }
}
