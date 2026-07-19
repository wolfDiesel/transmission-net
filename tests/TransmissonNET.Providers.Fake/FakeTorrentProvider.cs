using System.Collections.ObjectModel;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.Fake;

public sealed class FakeTorrentProvider : ITorrentProvider
{
    private TorrentProviderSettings _settings = new();

    public string Id => "fake";

    public string DisplayName => "Fake Provider";

    public bool IsLoginRequired => false;

    public bool IsLoggedIn => true;

    public ObservableCollection<TorrentSearchHit> Results { get; } = new();

    public IReadOnlyList<string> KnownMirrors { get; } = ["https://example.test/"];

    public Task LoginAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        TorrentProviderUiMarshal.ClearResults(Results, SynchronizationContext.Current);
        return Task.CompletedTask;
    }

    public Task SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sync = SynchronizationContext.Current;
        TorrentProviderUiMarshal.ReplaceResults(
            Results,
            [
                new TorrentSearchHit(
                    "1",
                    $"Fake result for {query}",
                    1024,
                    "https://example.test/1"),
            ],
            sync);
        return Task.CompletedTask;
    }

    public Task<byte[]> DownloadTorrentAsync(
        string hitId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Array.Empty<byte>());
    }

    public TorrentProviderSettings GetSettings() =>
        new()
        {
            RequestTimeoutSeconds = _settings.RequestTimeoutSeconds,
            BaseUrl = _settings.BaseUrl,
        };

    public void SetSettings(TorrentProviderSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = new TorrentProviderSettings
        {
            RequestTimeoutSeconds = Math.Clamp(settings.RequestTimeoutSeconds, 1, 600),
            BaseUrl = settings.BaseUrl,
        };
    }
}
