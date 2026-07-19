using System.Collections.ObjectModel;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Providers.Abstractions;
using Xunit;

namespace TransmissonNET.Application.Tests;

public sealed class SearchAcrossProvidersHandlerTests
{
    [Fact]
    public async Task HandleAsync_SearchesProvidersInParallel_AndReadsResults()
    {
        var catalog = new StubCatalog(
        [
            new StubProvider("a", "Alpha", [new TorrentSearchHit("1", "One", 10, "http://a/1")]),
            new StubProvider("b", "Beta", [new TorrentSearchHit("2", "Two", 20, "http://b/2")]),
        ]);
        var handler = new SearchAcrossProvidersHandler(catalog);

        var result = await handler.HandleAsync(new ProviderSearchRequestDto("ubuntu", ["a", "b"]));

        Assert.Equal(2, result.Hits.Count);
        Assert.Contains(result.Hits, h => h.ProviderId == "a" && h.Title == "One");
        Assert.Contains(result.Hits, h => h.ProviderId == "b" && h.Title == "Two");
        Assert.Empty(result.Errors);
        Assert.Single(catalog.GetById("a")!.Results);
        Assert.Single(catalog.GetById("b")!.Results);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderMissing_AddsError()
    {
        var handler = new SearchAcrossProvidersHandler(new StubCatalog([]));
        var result = await handler.HandleAsync(new ProviderSearchRequestDto("q", ["missing"]));

        Assert.Empty(result.Hits);
        Assert.Single(result.Errors);
    }

    private sealed class StubCatalog : ITorrentProviderCatalog
    {
        private readonly IReadOnlyList<ITorrentProvider> _providers;

        public StubCatalog(IReadOnlyList<ITorrentProvider> providers) => _providers = providers;

        public IReadOnlyList<string> LoadErrors => [];

        public IReadOnlyList<ITorrentProvider> GetProviders() => _providers;

        public ITorrentProvider? GetById(string providerId) =>
            _providers.FirstOrDefault(p => p.Id == providerId);
    }

    private sealed class StubProvider : ITorrentProvider
    {
        private readonly IReadOnlyList<TorrentSearchHit> _hits;
        private TorrentProviderSettings _settings = new();

        public StubProvider(string id, string displayName, IReadOnlyList<TorrentSearchHit> hits)
        {
            Id = id;
            DisplayName = displayName;
            _hits = hits;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public bool IsLoginRequired => false;
        public bool IsLoggedIn => true;
        public ObservableCollection<TorrentSearchHit> Results { get; } = new();
        public IReadOnlyList<string> KnownMirrors { get; } = [];

        public Task LoginAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            Results.Clear();
            return Task.CompletedTask;
        }

        public Task SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            Results.Clear();
            foreach (var hit in _hits)
                Results.Add(hit);
            return Task.CompletedTask;
        }

        public Task<byte[]> DownloadTorrentAsync(
            string hitId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Array.Empty<byte>());

        public TorrentProviderSettings GetSettings() =>
            new() { RequestTimeoutSeconds = _settings.RequestTimeoutSeconds };

        public void SetSettings(TorrentProviderSettings settings) =>
            _settings = settings;
    }
}
