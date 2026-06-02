using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Handlers;

namespace TransmissonNET.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTransmissonNetApplication(this IServiceCollection services)
    {
        services.AddScoped<GetSettingsHandler>();
        services.AddScoped<SaveSettingsHandler>();
        services.AddScoped<TestConnectionHandler>();
        services.AddScoped<GetTorrentsHandler>();
        services.AddScoped<GetTorrentDetailsHandler>();
        services.AddScoped<GetDaemonStatusHandler>();
        services.AddScoped<GetDaemonSessionSettingsHandler>();
        services.AddScoped<SaveDaemonSessionSettingsHandler>();
        services.AddScoped<ExecuteTorrentActionHandler>();
        services.AddScoped<ExecuteTorrentRenameBatchHandler>();
        services.AddScoped<InspectTorrentMetainfoHandler>();
        services.AddScoped<AddTorrentHandler>();
        return services;
    }
}
