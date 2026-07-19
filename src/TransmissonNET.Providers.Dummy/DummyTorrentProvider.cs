using System.Collections.ObjectModel;
using System.Text;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.Dummy;

public sealed class DummyTorrentProvider : ITorrentProvider
{
    private static readonly byte[] SampleTorrent = Encoding.ASCII.GetBytes(
        "d8:announce35:udp://tracker.example.test:1337/announce4:infod6:lengthi42e4:name12:dummy-sample12:piece lengthi16384e6:pieces20:aaaaaaaaaaaaaaaaaaaaee");

    private TorrentProviderSettings _settings = new();

    public string Id => "dummy";

    public string DisplayName => "Dummy";

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
        var q = query.Trim();
        TorrentProviderUiMarshal.ReplaceResults(
            Results,
            [
                new TorrentSearchHit(
                    "dummy-1",
                    $"[Dummy] {q}",
                    42,
                    "https://example.test/dummy/1"),
                new TorrentSearchHit(
                    "dummy-2",
                    $"[Dummy] alt {q}",
                    1024,
                    "https://example.test/dummy/2"),
            ],
            SynchronizationContext.Current);
        return Task.CompletedTask;
    }

    public Task<byte[]> DownloadTorrentAsync(
        string hitId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SampleTorrent);
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
            BaseUrl = string.IsNullOrWhiteSpace(settings.BaseUrl) ? "https://example.test/" : settings.BaseUrl.Trim(),
        };
    }
}
