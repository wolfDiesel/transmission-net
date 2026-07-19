using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.Kinozal;

public sealed class KinozalTorrentProvider : ITorrentProvider, IDisposable
{
    private readonly string _dataDirectory;
    private readonly string _settingsPath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private KinozalClient _client;
    private KinozalProviderSettings _settings;

    public KinozalTorrentProvider()
    {
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "TransmissonNET",
            "providers",
            "kinozal");
        Directory.CreateDirectory(_dataDirectory);
        _settingsPath = Path.Combine(_dataDirectory, "settings.json");
        _settings = LoadSettings();
        _client = CreateClient(_settings);
    }

    public string Id => "kinozal";

    public string DisplayName => "Kinozal";

    public bool IsLoginRequired => true;

    public bool IsLoggedIn => _client.IsLoggedIn;

    public ObservableCollection<TorrentSearchHit> Results { get; } = new();

    public IReadOnlyList<string> KnownMirrors => KinozalMirrors.FallbackUrls;

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
        var baseUrl = KinozalMirrors.NormalizeBaseUrl(
            string.IsNullOrWhiteSpace(settings.BaseUrl) ? KinozalMirrors.DefaultBaseUrl : settings.BaseUrl);

        _settings = new KinozalProviderSettings
        {
            RequestTimeoutSeconds = timeout,
            BaseUrl = baseUrl,
        };
        SaveSettings(_settings);
        _client.Dispose();
        _client = CreateClient(_settings);
        KinozalLog.Info($"Settings applied. Timeout={timeout}, BaseUrl={baseUrl}");
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
                    ?? throw new InvalidOperationException("No application window available for Kinozal login.");
                var window = new KinozalLoginWindow();
                var accepted = await window.ShowDialog<bool?>(owner);
                if (accepted != true)
                    return ((string Username, string Password)?)null;

                return (window.Username, window.Password);
            });

            cancellationToken.ThrowIfCancellationRequested();
            if (credentials is null)
                return;

            await _client.LoginAsync(credentials.Value.Username, credentials.Value.Password, cancellationToken);
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

    private KinozalClient CreateClient(KinozalProviderSettings settings)
    {
        var client = new KinozalClient(settings.BaseUrl, _dataDirectory);
        client.SetTimeout(TimeSpan.FromSeconds(Math.Max(settings.RequestTimeoutSeconds + 5, 10)));
        return client;
    }

    private KinozalProviderSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return KinozalProviderSettings.CreateDefault();

            var loaded = JsonSerializer.Deserialize<KinozalProviderSettings>(File.ReadAllText(_settingsPath));
            if (loaded is null)
                return KinozalProviderSettings.CreateDefault();

            loaded.RequestTimeoutSeconds = Math.Clamp(loaded.RequestTimeoutSeconds, 1, 600);
            loaded.BaseUrl = KinozalMirrors.NormalizeBaseUrl(
                string.IsNullOrWhiteSpace(loaded.BaseUrl) ? KinozalMirrors.DefaultBaseUrl : loaded.BaseUrl);
            return loaded;
        }
        catch (Exception ex)
        {
            KinozalLog.Error("Failed to load settings", ex);
            return KinozalProviderSettings.CreateDefault();
        }
    }

    private void SaveSettings(KinozalProviderSettings settings)
    {
        try
        {
            File.WriteAllText(
                _settingsPath,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            KinozalLog.Error("Failed to save settings", ex);
        }
    }

    private static Window? GetOwnerWindow()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}

internal sealed class KinozalProviderSettings
{
    public int RequestTimeoutSeconds { get; set; } = 10;

    public string BaseUrl { get; set; } = KinozalMirrors.DefaultBaseUrl;

    public static KinozalProviderSettings CreateDefault() => new();
}
