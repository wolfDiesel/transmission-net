using TransmissonNET.Application.Contracts;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Settings;

internal static class SettingsMapper
{
    public static AppSettingsDto ToDto(AppSettings settings, bool maskPassword) =>
        new(
            new DaemonConnectionDto(
                settings.Daemon.Host,
                settings.Daemon.Port,
                settings.Daemon.RpcPath,
                settings.Daemon.Username,
                maskPassword ? null : settings.Daemon.Password),
            new UiSettingsDto(
                settings.Ui.RefreshIntervalSeconds,
                settings.Ui.WindowWidth,
                settings.Ui.WindowHeight,
                TorrentTableSettingsMapper.ToDto(settings.Ui.TorrentTable),
                settings.Ui.ColorScheme,
                settings.Ui.Appearance,
                settings.Ui.DownloadDirHistory,
                settings.Ui.TorrentFileAssociation,
                settings.Ui.TrayEnabled,
                settings.Ui.MinimizeToTray,
                settings.Ui.CloseToTray));

    public static AppSettings ToDomain(AppSettingsDto dto, AppSettings? existing = null)
    {
        var password = string.IsNullOrEmpty(dto.Daemon.Password)
            ? existing?.Daemon.Password ?? string.Empty
            : dto.Daemon.Password;

        return new AppSettings(
            new DaemonConnection(
                dto.Daemon.Host,
                dto.Daemon.Port,
                dto.Daemon.RpcPath,
                dto.Daemon.Username,
                password),
            new UiSettings(
                dto.Ui.RefreshIntervalSeconds,
                dto.Ui.WindowWidth,
                dto.Ui.WindowHeight,
                TorrentTableSettingsMapper.ToDomain(dto.Ui.TorrentTable, existing?.Ui.TorrentTable),
                NormalizeColorScheme(dto.Ui.ColorScheme),
                NormalizeAppearance(dto.Ui.Appearance),
                dto.Ui.DownloadDirHistory ?? existing?.Ui.DownloadDirHistory,
                TorrentFileAssociationStatuses.Normalize(
                    dto.Ui.TorrentFileAssociation ?? existing?.Ui.TorrentFileAssociation),
                dto.Ui.TrayEnabled,
                dto.Ui.MinimizeToTray,
                dto.Ui.CloseToTray));
    }

    public static DaemonConnection ToConnection(DaemonConnectionDto dto, AppSettings? existing = null)
    {
        var password = string.IsNullOrEmpty(dto.Password)
            ? existing?.Daemon.Password ?? string.Empty
            : dto.Password;

        return new DaemonConnection(
            dto.Host,
            dto.Port,
            dto.RpcPath,
            dto.Username,
            password);
    }

    private static string NormalizeColorScheme(string? value) =>
        !string.IsNullOrWhiteSpace(value) && UiColorSchemes.All.Contains(value)
            ? value
            : UiColorSchemes.Default;

    private static string NormalizeAppearance(string? value) =>
        !string.IsNullOrWhiteSpace(value) && UiAppearances.All.Contains(value)
            ? value
            : UiAppearances.Default;
}
