using System.Collections.Concurrent;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Application.Handlers;

public sealed class SearchAcrossProvidersHandler
{
    private readonly ITorrentProviderCatalog _catalog;

    public SearchAcrossProvidersHandler(ITorrentProviderCatalog catalog)
    {
        _catalog = catalog;
    }

    public async Task<ProviderSearchResultDto> HandleAsync(
        ProviderSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ArgumentException("Search query is required.", nameof(request));

        if (request.ProviderIds is null || request.ProviderIds.Count == 0)
            throw new ArgumentException("At least one provider must be selected.", nameof(request));

        var query = request.Query.Trim();
        var errors = new ConcurrentBag<string>();
        var providers = new List<ITorrentProvider>();

        foreach (var providerId in request.ProviderIds.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var provider = _catalog.GetById(providerId);
            if (provider is null)
            {
                errors.Add($"Provider '{providerId}' was not found.");
                continue;
            }

            if (provider.IsLoginRequired && !provider.IsLoggedIn)
            {
                errors.Add($"{provider.DisplayName}: login required.");
                continue;
            }

            providers.Add(provider);
        }

        var tasks = providers.Select(async provider =>
        {
            try
            {
                await provider.SearchAsync(query, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                errors.Add($"{provider.DisplayName}: {ex.Message}");
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var hits = providers
            .SelectMany(provider => provider.Results.Select(hit => new ProviderSearchHitDto(
                provider.Id,
                provider.DisplayName,
                hit.Id,
                hit.Title,
                hit.SizeBytes,
                hit.DetailUrl)))
            .ToList();

        return new ProviderSearchResultDto(hits, errors.ToList());
    }
}
