using TransmissonNET.Domain;

namespace TransmissonNET.Application.Abstractions;

public interface ITransmissionClient
{
    Task<SessionInfo> GetSessionAsync(DaemonConnection connection, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Torrent>> GetTorrentsAsync(
        DaemonConnection connection,
        CancellationToken cancellationToken = default);

    Task<TorrentStatusCounts> GetTorrentStatusCountsAsync(
        DaemonConnection connection,
        CancellationToken cancellationToken = default);

    Task<TorrentDetails?> GetTorrentDetailsAsync(
        DaemonConnection connection,
        int id,
        CancellationToken cancellationToken = default);

    Task<TransmissionDaemonSettings> GetDaemonSessionSettingsAsync(
        DaemonConnection connection,
        CancellationToken cancellationToken = default);

    Task SetDaemonSessionSettingsAsync(
        DaemonConnection connection,
        TransmissionDaemonSettings settings,
        CancellationToken cancellationToken = default);

    Task StartTorrentsAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        CancellationToken cancellationToken = default);

    Task StopTorrentsAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        CancellationToken cancellationToken = default);

    Task RemoveTorrentsAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        bool deleteLocalData,
        CancellationToken cancellationToken = default);

    Task VerifyTorrentsAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        CancellationToken cancellationToken = default);

    Task SetTorrentBandwidthPriorityAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        TorrentBandwidthPriority priority,
        CancellationToken cancellationToken = default);

    Task SetTorrentFilePriorityAsync(
        DaemonConnection connection,
        int torrentId,
        IReadOnlyList<int> fileIndices,
        TorrentBandwidthPriority priority,
        CancellationToken cancellationToken = default);

    Task SetTorrentLocationAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        string location,
        bool move,
        CancellationToken cancellationToken = default);

    Task RenameTorrentPathAsync(
        DaemonConnection connection,
        int id,
        string path,
        string name,
        CancellationToken cancellationToken = default);

    Task<TorrentAddResult> AddTorrentAsync(
        DaemonConnection connection,
        byte[] metainfo,
        string? downloadDir,
        bool paused,
        CancellationToken cancellationToken = default);
}
