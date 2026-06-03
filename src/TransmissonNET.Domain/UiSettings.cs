namespace TransmissonNET.Domain;

public sealed record UiSettings(
    int RefreshIntervalSeconds,
    int WindowWidth,
    int WindowHeight,
    TorrentTableSettings TorrentTable,
    string ColorScheme = UiColorSchemes.Default,
    string Appearance = UiAppearances.Default,
    IReadOnlyList<string>? DownloadDirHistory = null,
    string TorrentFileAssociation = TorrentFileAssociationStatuses.NotAsked);
