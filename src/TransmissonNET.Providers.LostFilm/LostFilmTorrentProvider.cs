using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.LostFilm;

public sealed class LostFilmTorrentProvider : ITorrentProvider, IDisposable
{
    private readonly string _dataDirectory;
    private readonly string _settingsPath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private LostFilmClient _client;
    private LostFilmProviderSettings _settings;

    public LostFilmTorrentProvider()
    {
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "TransmissonNET",
            "providers",
            "lostfilm");
        Directory.CreateDirectory(_dataDirectory);
        _settingsPath = Path.Combine(_dataDirectory, "settings.json");
        _settings = LoadSettings();
        _client = CreateClient(_settings);
    }

    public string Id => "lostfilm";

    public string DisplayName => "LostFilm";

    public bool IsLoginRequired => true;

    public bool IsLoggedIn => _client.IsLoggedIn;

    public ObservableCollection<TorrentSearchHit> Results { get; } = new();

    public IReadOnlyList<string> KnownMirrors => LostFilmMirrors.FallbackUrls;

    public TorrentProviderSettings GetSettings() =>
        new()
        {
            RequestTimeoutSeconds = _settings.RequestTimeoutSeconds,
            BaseUrl = _settings.BaseUrl,
            PreferredQuality = _settings.PreferredQuality,
            MaxSeriesExpand = _settings.MaxSeriesExpand,
        };

    public void SetSettings(TorrentProviderSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var timeout = Math.Clamp(settings.RequestTimeoutSeconds, 1, 600);
        var expand = settings.MaxSeriesExpand <= 0 ? 3 : Math.Clamp(settings.MaxSeriesExpand, 1, 20);
        var quality = string.IsNullOrWhiteSpace(settings.PreferredQuality)
            ? "1080"
            : LostFilmHtmlParser.NormalizeQuality(settings.PreferredQuality);
        var baseUrl = LostFilmMirrors.NormalizeBaseUrl(
            string.IsNullOrWhiteSpace(settings.BaseUrl) ? LostFilmMirrors.DefaultBaseUrl : settings.BaseUrl);

        _settings = new LostFilmProviderSettings
        {
            RequestTimeoutSeconds = timeout,
            BaseUrl = baseUrl,
            PreferredQuality = quality,
            MaxSeriesExpand = expand,
        };
        SaveSettings(_settings);
        _client.Dispose();
        _client = CreateClient(_settings);
        LostFilmLog.Info(
            $"Settings applied. Timeout={timeout}, BaseUrl={baseUrl}, Quality={quality}, Expand={expand}");
    }

    public async Task LoginAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_client.IsLoggedIn)
                return;

            var credentials = await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var owner = GetOwnerWindow()
                    ?? throw new InvalidOperationException("No application window available for LostFilm login.");
                var window = new LostFilmLoginWindow();
                var accepted = await window.ShowDialog<bool?>(owner);
                if (accepted != true)
                    return ((string Email, string Password, string Cookie)?)null;

                return (window.Email, window.Password, window.SessionCookie);
            });

            cancellationToken.ThrowIfCancellationRequested();
            if (credentials is null)
                return;

            if (!string.IsNullOrWhiteSpace(credentials.Value.Cookie))
            {
                _client.LoginWithSessionCookie(credentials.Value.Cookie);
                return;
            }

            await _client.LoginAsync(credentials.Value.Email, credentials.Value.Password, cancellationToken);
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
            _client.Logout();
            TorrentProviderUiMarshal.ClearResults(Results, SynchronizationContext.Current);
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
            var hits = await _client.SearchAsync(query, _settings.MaxSeriesExpand, timeoutCts.Token);
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
            return await _client.DownloadTorrentAsync(hitId, _settings.PreferredQuality, timeoutCts.Token);
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

    private LostFilmClient CreateClient(LostFilmProviderSettings settings)
    {
        var client = new LostFilmClient(settings.BaseUrl, _dataDirectory);
        client.SetTimeout(TimeSpan.FromSeconds(Math.Max(settings.RequestTimeoutSeconds + 5, 10)));
        return client;
    }

    private LostFilmProviderSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return LostFilmProviderSettings.CreateDefault();

            var json = File.ReadAllText(_settingsPath);
            var loaded = JsonSerializer.Deserialize<LostFilmProviderSettings>(json);
            if (loaded is null)
                return LostFilmProviderSettings.CreateDefault();

            loaded.RequestTimeoutSeconds = Math.Clamp(loaded.RequestTimeoutSeconds, 1, 600);
            loaded.MaxSeriesExpand = loaded.MaxSeriesExpand <= 0
                ? 3
                : Math.Clamp(loaded.MaxSeriesExpand, 1, 20);
            loaded.PreferredQuality = string.IsNullOrWhiteSpace(loaded.PreferredQuality)
                ? "1080"
                : LostFilmHtmlParser.NormalizeQuality(loaded.PreferredQuality);
            loaded.BaseUrl = LostFilmMirrors.NormalizeBaseUrl(
                string.IsNullOrWhiteSpace(loaded.BaseUrl) ? LostFilmMirrors.DefaultBaseUrl : loaded.BaseUrl);
            return loaded;
        }
        catch (Exception ex)
        {
            LostFilmLog.Error("Failed to load settings", ex);
            return LostFilmProviderSettings.CreateDefault();
        }
    }

    private void SaveSettings(LostFilmProviderSettings settings)
    {
        try
        {
            File.WriteAllText(
                _settingsPath,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            LostFilmLog.Error("Failed to save settings", ex);
        }
    }

    private static Window? GetOwnerWindow()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}

internal sealed class LostFilmProviderSettings
{
    public int RequestTimeoutSeconds { get; set; } = 10;

    public string BaseUrl { get; set; } = LostFilmMirrors.DefaultBaseUrl;

    public string PreferredQuality { get; set; } = "1080";

    public int MaxSeriesExpand { get; set; } = 3;

    public static LostFilmProviderSettings CreateDefault() => new();
}
