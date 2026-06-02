using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TransmissonNET.Application;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Domain;

namespace TransmissonNET.Infrastructure.Rpc;

public sealed class TransmissionRpcClient(HttpClient http) : ITransmissionClient
{
    private const string SessionHeader = "X-Transmission-Session-Id";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly RpcMethodNaming _methodNaming = new();
    private string? _sessionId;

    public async Task<SessionInfo> GetSessionAsync(
        DaemonConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = await CallAsync(
                connection,
                _methodNaming.SessionGet,
                new { fields = new[] { "version", "rpc-version" } },
                cancellationToken);

            var args = doc.RootElement.GetProperty("arguments");
            var rpcVersion = args.TryGetProperty("rpc-version", out var rv)
                ? rv.GetInt32()
                : args.GetProperty("rpc_version").GetInt32();
            var version = args.GetProperty("version").GetString() ?? string.Empty;

            _methodNaming.SetRpcVersion(rpcVersion);

            var (downloadSpeed, uploadSpeed) = await TryReadSessionSpeedsAsync(connection, cancellationToken);

            return new SessionInfo(rpcVersion, version, downloadSpeed, uploadSpeed);
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    public async Task<TransmissionDaemonSettings> GetDaemonSessionSettingsAsync(
        DaemonConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_sessionId is null)
                await GetSessionAsync(connection, cancellationToken);

            using var doc = await CallAsync(
                connection,
                _methodNaming.SessionGet,
                new { fields = SessionSettingsMapper.FieldNames },
                cancellationToken);

            return SessionSettingsMapper.Map(doc.RootElement.GetProperty("arguments"));
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    public async Task SetDaemonSessionSettingsAsync(
        DaemonConnection connection,
        TransmissionDaemonSettings settings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_sessionId is null)
                await GetSessionAsync(connection, cancellationToken);

            await CallAsync(
                connection,
                _methodNaming.SessionSet,
                SessionSettingsMapper.ToRpcArguments(settings),
                cancellationToken);
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    public Task StartTorrentsAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        CancellationToken cancellationToken = default) =>
        CallTorrentIdsActionAsync(connection, _methodNaming.TorrentStart, ids, cancellationToken);

    public Task StopTorrentsAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        CancellationToken cancellationToken = default) =>
        CallTorrentIdsActionAsync(connection, _methodNaming.TorrentStop, ids, cancellationToken);

    public Task VerifyTorrentsAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        CancellationToken cancellationToken = default) =>
        CallTorrentIdsActionAsync(connection, _methodNaming.TorrentVerify, ids, cancellationToken);

    public async Task RemoveTorrentsAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        bool deleteLocalData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureSessionAsync(connection, cancellationToken);
            await CallAsync(
                connection,
                _methodNaming.TorrentRemove,
                new { ids, deleteLocalData },
                cancellationToken);
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    public async Task SetTorrentBandwidthPriorityAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        TorrentBandwidthPriority priority,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureSessionAsync(connection, cancellationToken);
            await CallAsync(
                connection,
                _methodNaming.TorrentSet,
                new Dictionary<string, object>
                {
                    ["ids"] = ids,
                    ["bandwidthPriority"] = (int)priority,
                },
                cancellationToken);
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    public async Task SetTorrentLocationAsync(
        DaemonConnection connection,
        IReadOnlyList<int> ids,
        string location,
        bool move,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureSessionAsync(connection, cancellationToken);
            await CallAsync(
                connection,
                _methodNaming.TorrentSetLocation,
                new { ids, location, move },
                cancellationToken);
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    public async Task RenameTorrentPathAsync(
        DaemonConnection connection,
        int id,
        string path,
        string name,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureSessionAsync(connection, cancellationToken);
            await CallAsync(
                connection,
                _methodNaming.TorrentRenamePath,
                new { ids = new[] { id }, path, name },
                cancellationToken);
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    public async Task<IReadOnlyList<Torrent>> GetTorrentsAsync(
        DaemonConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_sessionId is null)
                await GetSessionAsync(connection, cancellationToken);

            using var doc = await CallAsync(
                connection,
                _methodNaming.TorrentGet,
                new { fields = TorrentMapper.Fields },
                cancellationToken);

            return TorrentMapper.MapTorrents(doc.RootElement.GetProperty("arguments"));
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    public async Task<TorrentStatusCounts> GetTorrentStatusCountsAsync(
        DaemonConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_sessionId is null)
                await GetSessionAsync(connection, cancellationToken);

            using var doc = await CallAsync(
                connection,
                _methodNaming.TorrentGet,
                new { fields = TorrentStatusCountsMapper.RpcFields },
                cancellationToken);

            return TorrentStatusCountsMapper.Map(doc.RootElement.GetProperty("arguments"));
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    public async Task<TorrentAddResult> AddTorrentAsync(
        DaemonConnection connection,
        byte[] metainfo,
        string? downloadDir,
        bool paused,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureSessionAsync(connection, cancellationToken);

            var arguments = new Dictionary<string, object>
            {
                ["metainfo"] = Convert.ToBase64String(metainfo),
                ["paused"] = paused,
            };

            if (!string.IsNullOrWhiteSpace(downloadDir))
                arguments["download-dir"] = downloadDir.Trim();

            using var doc = await CallAsync(
                connection,
                _methodNaming.TorrentAdd,
                arguments,
                cancellationToken);

            return TorrentAddMapper.MapAddedTorrent(doc.RootElement.GetProperty("arguments"));
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    public async Task<TorrentDetails?> GetTorrentDetailsAsync(
        DaemonConnection connection,
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_sessionId is null)
                await GetSessionAsync(connection, cancellationToken);

            using var doc = await CallAsync(
                connection,
                _methodNaming.TorrentGet,
                new { ids = new[] { id }, fields = TorrentDetailsMapper.Fields },
                cancellationToken);

            var torrents = TorrentDetailsMapper.MapTorrents(doc.RootElement.GetProperty("arguments"));
            return torrents.FirstOrDefault();
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    private async Task CallTorrentIdsActionAsync(
        DaemonConnection connection,
        string method,
        IReadOnlyList<int> ids,
        CancellationToken cancellationToken)
    {
        try
        {
            await EnsureSessionAsync(connection, cancellationToken);
            await CallAsync(connection, method, new { ids }, cancellationToken);
        }
        catch (TransmissionRpcException ex)
        {
            throw new DaemonConnectionException(ex.Message, ex);
        }
    }

    private async Task EnsureSessionAsync(DaemonConnection connection, CancellationToken cancellationToken)
    {
        if (_sessionId is null)
            await GetSessionAsync(connection, cancellationToken);
    }

    private async Task<JsonDocument> CallAsync(
        DaemonConnection connection,
        string method,
        object arguments,
        CancellationToken cancellationToken)
    {
        var response = await SendRpcAsync(connection, method, arguments, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            RememberSessionId(response);
            response.Dispose();
            response = await SendRpcAsync(connection, method, arguments, cancellationToken);
        }

        using (response)
        {
            RememberSessionId(response);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new TransmissionRpcException($"RPC HTTP {(int)response.StatusCode}: {body}");

            using var doc = JsonDocument.Parse(body);
            var result = doc.RootElement.GetProperty("result").GetString();

            if (!string.Equals(result, "success", StringComparison.OrdinalIgnoreCase))
                throw new TransmissionRpcException($"RPC failed: {body}");

            return JsonDocument.Parse(body);
        }
    }

    private async Task<(long Download, long Upload)> TryReadSessionSpeedsAsync(
        DaemonConnection connection,
        CancellationToken cancellationToken)
    {
        try
        {
            using var doc = await CallAsync(
                connection,
                _methodNaming.SessionGet,
                new { fields = new[] { "download-speed", "upload-speed" } },
                cancellationToken);

            var args = doc.RootElement.GetProperty("arguments");
            return (
                ReadInt64(args, "download-speed", "downloadSpeed"),
                ReadInt64(args, "upload-speed", "uploadSpeed"));
        }
        catch (TransmissionRpcException)
        {
            return (0, 0);
        }
    }

    private static long ReadInt64(JsonElement args, string kebabName, string camelName) =>
        args.TryGetProperty(kebabName, out var kebab)
            ? kebab.GetInt64()
            : args.TryGetProperty(camelName, out var camel)
                ? camel.GetInt64()
                : 0;

    private void RememberSessionId(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(SessionHeader, out var values))
            _sessionId = values.FirstOrDefault();
    }

    private async Task<HttpResponseMessage> SendRpcAsync(
        DaemonConnection connection,
        string method,
        object arguments,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new { method, arguments }, JsonOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, connection.RpcUrl)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrEmpty(connection.Username))
        {
            var token = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{connection.Username}:{connection.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
        }

        if (!string.IsNullOrEmpty(_sessionId))
            request.Headers.TryAddWithoutValidation(SessionHeader, _sessionId);

        return await http.SendAsync(request, cancellationToken);
    }
}
