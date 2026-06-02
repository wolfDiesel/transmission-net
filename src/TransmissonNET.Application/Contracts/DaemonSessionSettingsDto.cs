namespace TransmissonNET.Application.Contracts;

public sealed record DaemonSessionSettingsDto(
    string DownloadDir,
    string IncompleteDir,
    bool IncompleteDirEnabled,
    bool TrashOriginalTorrentFiles,
    int PeerLimitGlobal,
    int PeerLimitPerTorrent,
    int SpeedLimitDownKbps,
    int SpeedLimitUpKbps,
    bool SpeedLimitDownEnabled,
    bool SpeedLimitUpEnabled,
    double SeedRatioLimit,
    bool SeedRatioLimited,
    int IdleSeedingLimitMinutes,
    bool IdleSeedingLimitEnabled);
