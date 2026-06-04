namespace TransmissonNET.Domain;

public sealed record UiSettings(
    int RefreshIntervalSeconds,
    int WindowWidth,
    int WindowHeight,
    TorrentTableSettings TorrentTable,
    string ColorScheme = UiColorSchemes.Default,
    string Appearance = UiAppearances.Default,
    IReadOnlyList<string>? DownloadDirHistory = null,
    string TorrentFileAssociation = TorrentFileAssociationStatuses.NotAsked,
    bool TrayEnabled = true,
    bool MinimizeToTray = false,
    bool CloseToTray = true,
    string Language = UiLanguages.Default);
