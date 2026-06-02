using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Infrastructure.Rpc;
using TransmissonNET.Infrastructure.Settings;

namespace TransmissonNET.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTransmissonNetInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient<ITransmissionClient, TransmissionRpcClient>();
        services.AddSingleton<ISettingsStore, JsonSettingsStore>();
        return services;
    }
}
