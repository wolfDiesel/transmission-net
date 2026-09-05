using System.Collections.ObjectModel;
using System.Text.Json;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.RuTracker;

public sealed class RuTrackerTorrentProvider : ITorrentProvider, IDisposable
{
    private readonly string _dataDirectory;
    private readonly string _settingsPath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly IProviderUiHost _uiHost;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Reserved for F6 session storage.")]
    private readonly IProviderSessionStore _sessionStore;
    private RuTrackerClient _client;
    private TorrentProviderSettings _settings;

    public RuTrackerTorrentProvider(
        TorrentProviderSettings settings,
        IProviderUiHost uiHost,
        IProviderSessionStore sessionStore)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(uiHost);
        ArgumentNullException.ThrowIfNull(sessionStore);
        _uiHost = uiHost;
        _sessionStore = sessionStore;
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "TransmissonNET",
            "providers",
            "rutracker");
        Directory.CreateDirectory(_dataDirectory);
        _settingsPath = Path.Combine(_dataDirectory, "settings.json");
        _settings = LoadSettings();
        _client = CreateClient(_settings);
    }

    public string Id => "rutracker";

    public string DisplayName => "RuTracker";

    public bool IsLoginRequired => true;

    public bool IsLoggedIn => _client.IsLoggedIn;

    public ObservableCollection<TorrentSearchHit> Results { get; } = new();

    public IReadOnlyList<string> KnownMirrors => RuTrackerMirrors.FallbackUrls;

    public TorrentProviderSettings GetSettings() =>
        new()
        {
            RequestTimeoutSeconds = _settings.RequestTimeoutSeconds,
            BaseUrl = _settings.BaseUrl,
        };

    public void SetSettings(TorrentProviderSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var timeout = Math.Clamp(settings.RequestTimeoutSeconds, 1, 600);
        var baseUrl = RuTrackerMirrors.NormalizeBaseUrl(
            string.IsNullOrWhiteSpace(settings.BaseUrl) ? RuTrackerMirrors.DefaultBaseUrl : settings.BaseUrl);
        _settings = new TorrentProviderSettings
        {
            RequestTimeoutSeconds = timeout,
            BaseUrl = baseUrl,
        };
        SaveSettings(_settings);
        _client.Dispose();
        _client = CreateClient(_settings);
        RuTrackerLog.Info($"Settings applied. RequestTimeoutSeconds={timeout}, BaseUrl={baseUrl}");
    }

    public async Task LoginAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_client.IsLoggedIn)
                return;

            var preferred = RuTrackerMirrors.NormalizeBaseUrl(_settings.BaseUrl);
            _client.SetBaseUrl(preferred);

            var accepted = await _uiHost.LoginAsync("rutracker", preferred, _dataDirectory, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            if (accepted is null)
                return;

            _client.ImportWebSession(accepted.Cookies, accepted.UserAgent);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _client.ClearSession();
            TorrentProviderUiMarshal.ClearResults(Results, SynchronizationContext.Current);
            RuTrackerLog.Info("Logged out; session cleared");
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var sync = SynchronizationContext.Current;
        TorrentProviderUiMarshal.ClearResults(Results, sync);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_settings.RequestTimeoutSeconds));
            var hits = await _client.SearchAsync(query, timeoutCts.Token);
            TorrentProviderUiMarshal.ReplaceResults(Results, hits, sync);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<byte[]> DownloadTorrentAsync(
        string hitId,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_settings.RequestTimeoutSeconds));
            return await _client.DownloadTorrentAsync(hitId, timeoutCts.Token);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _gate.Dispose();
    }

    private RuTrackerClient CreateClient(TorrentProviderSettings settings)
    {
        var baseUrl = string.IsNullOrWhiteSpace(settings.BaseUrl)
            ? RuTrackerClient.DefaultBaseUrl
            : settings.BaseUrl.TrimEnd('/');
        var client = new RuTrackerClient(baseUrl: baseUrl, dataDirectory: _dataDirectory);
        client.SetTimeout(TimeSpan.FromSeconds(Math.Max(settings.RequestTimeoutSeconds + 5, 10)));
        return client;
    }

    private TorrentProviderSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return new TorrentProviderSettings { BaseUrl = RuTrackerMirrors.DefaultBaseUrl };

            var json = File.ReadAllText(_settingsPath);
            var loaded = JsonSerializer.Deserialize<TorrentProviderSettings>(json);
            if (loaded is null)
                return new TorrentProviderSettings { BaseUrl = RuTrackerMirrors.DefaultBaseUrl };

            loaded.RequestTimeoutSeconds = Math.Clamp(loaded.RequestTimeoutSeconds, 1, 600);
            loaded.BaseUrl = RuTrackerMirrors.NormalizeBaseUrl(
                string.IsNullOrWhiteSpace(loaded.BaseUrl) ? RuTrackerMirrors.DefaultBaseUrl : loaded.BaseUrl);
            return loaded;
        }
        catch (Exception ex)
        {
            RuTrackerLog.Error("Failed to load settings", ex);
            return new TorrentProviderSettings { BaseUrl = RuTrackerMirrors.DefaultBaseUrl };
        }
    }

    private void SaveSettings(TorrentProviderSettings settings)
    {
        try
        {
            File.WriteAllText(
                _settingsPath,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            RuTrackerLog.Error("Failed to save settings", ex);
        }
    }
}
