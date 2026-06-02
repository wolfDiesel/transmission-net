using TransmissonNET.Application.Contracts;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Settings;

internal static class DaemonSessionSettingsMapper
{
    public static DaemonSessionSettingsDto ToDto(TransmissionDaemonSettings settings) =>
        new(
            settings.DownloadDir,
            settings.IncompleteDir,
            settings.IncompleteDirEnabled,
            settings.TrashOriginalTorrentFiles,
            settings.PeerLimitGlobal,
            settings.PeerLimitPerTorrent,
            settings.SpeedLimitDownKbps,
            settings.SpeedLimitUpKbps,
            settings.SpeedLimitDownEnabled,
            settings.SpeedLimitUpEnabled,
            settings.SeedRatioLimit,
            settings.SeedRatioLimited,
            settings.IdleSeedingLimitMinutes,
            settings.IdleSeedingLimitEnabled);

    public static TransmissionDaemonSettings ToDomain(DaemonSessionSettingsDto dto) =>
        new(
            dto.DownloadDir.Trim(),
            dto.IncompleteDir.Trim(),
            dto.IncompleteDirEnabled,
            dto.TrashOriginalTorrentFiles,
            dto.PeerLimitGlobal,
            dto.PeerLimitPerTorrent,
            dto.SpeedLimitDownKbps,
            dto.SpeedLimitUpKbps,
            dto.SpeedLimitDownEnabled,
            dto.SpeedLimitUpEnabled,
            dto.SeedRatioLimit,
            dto.SeedRatioLimited,
            dto.IdleSeedingLimitMinutes,
            dto.IdleSeedingLimitEnabled);
}
