using TransmissonNET.Domain;

namespace TransmissonNET.Application.Contracts;

public sealed record AppSettingsDto(DaemonConnectionDto Daemon, UiSettingsDto Ui);

public sealed record DaemonConnectionDto(
    string Host,
    int Port,
    string RpcPath,
    string Username,
    string? Password);

public sealed record UiSettingsDto(
    int RefreshIntervalSeconds,
    int WindowWidth,
    int WindowHeight,
    TorrentTableSettingsDto TorrentTable,
    string ColorScheme = UiColorSchemes.Default,
    string Appearance = UiAppearances.Default,
    IReadOnlyList<string>? DownloadDirHistory = null);
