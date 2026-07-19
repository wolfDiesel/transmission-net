using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Application.Abstractions;

public interface ITorrentProviderCatalog
{
    IReadOnlyList<ITorrentProvider> GetProviders();

    ITorrentProvider? GetById(string providerId);

    IReadOnlyList<string> LoadErrors { get; }
}
