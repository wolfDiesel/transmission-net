using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Application.Contracts;

public sealed record ProviderSearchRequestDto(
    string Query,
    IReadOnlyList<string> ProviderIds);

public sealed record ProviderSearchHitDto(
    string ProviderId,
    string ProviderDisplayName,
    string HitId,
    string Title,
    long? SizeBytes,
    string? DetailUrl);

public sealed record ProviderSearchResultDto(
    IReadOnlyList<ProviderSearchHitDto> Hits,
    IReadOnlyList<string> Errors);
