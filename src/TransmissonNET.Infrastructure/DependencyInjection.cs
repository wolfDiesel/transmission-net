using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Infrastructure.Desktop;
using TransmissonNET.Infrastructure.Rpc;
using TransmissonNET.Infrastructure.Settings;
using TransmissonNET.Infrastructure.TorrentProviders;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTransmissonNetInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient<ITransmissionClient, TransmissionRpcClient>();
        services.AddSingleton<ISettingsStore, JsonSettingsStore>();
        services.AddSingleton<ITorrentFileAssociationService>(sp =>
            OperatingSystem.IsLinux()
                ? new LinuxTorrentFileAssociationService()
                : new NullTorrentFileAssociationService());
        services.AddSingleton<ITorrentProviderCatalog>(sp =>
        {
            var providersDir = Path.Combine(AppContext.BaseDirectory, "providers");
            return TorrentProviderLoader.LoadFromDirectory(providersDir, sp);
        });
        services.AddSingleton<IProviderSessionStore, NullProviderSessionStore>();
        services.AddTransient<TorrentProviderSettings>();
        return services;
    }
}
