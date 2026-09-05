using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.App.Avalonia.Providers;

/// <summary>
/// Dispatches <see cref="IProviderUiHost.LoginAsync"/> to the host that owns the
/// requested provider id. Concrete hosts are injected explicitly so the router
/// itself can be registered as the single <see cref="IProviderUiHost"/>.
/// </summary>
public sealed class ProviderUiHostRouter : IProviderUiHost
{
    private readonly IReadOnlyList<IProviderUiHost> _hosts;

    public ProviderUiHostRouter(
        RuTrackerProviderUiHost rutracker,
        LostFilmProviderUiHost lostfilm,
        KinozalProviderUiHost kinozal)
    {
        _hosts = new IProviderUiHost[] { rutracker, lostfilm, kinozal };
    }

    public async Task<ProviderLoginResult?> LoginAsync(
        string providerId,
        string baseUrl,
        string dataDirectory,
        CancellationToken cancellationToken = default)
    {
        foreach (var host in _hosts)
        {
            try
            {
                return await host.LoginAsync(providerId, baseUrl, dataDirectory, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // Host does not own this provider; try the next one.
            }
        }

        throw new InvalidOperationException($"No UI host registered for provider '{providerId}'.");
    }
}