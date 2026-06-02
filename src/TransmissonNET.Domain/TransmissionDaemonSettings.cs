namespace TransmissonNET.Domain;

public sealed record TransmissionDaemonSettings(
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
