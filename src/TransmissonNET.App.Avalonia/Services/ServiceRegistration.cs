using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application;
using TransmissonNET.App.Avalonia.Providers;
using TransmissonNET.App.Avalonia.ViewModels;
using TransmissonNET.Infrastructure;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.App.Avalonia.Services;

internal static class ServiceRegistration
{
    public static IServiceCollection AddAvaloniaApp(this IServiceCollection services)
    {
        services.AddTransmissonNetApplication();
        services.AddTransmissonNetInfrastructure();
        services.AddSingleton<RuTrackerProviderUiHost>();
        services.AddSingleton<LostFilmProviderUiHost>();
        services.AddSingleton<KinozalProviderUiHost>();
        services.AddSingleton<IProviderUiHost, ProviderUiHostRouter>();
        services.AddSingleton<HandlerInvoker>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<AppToastService>();
        services.AddSingleton<LocalizationService>();
        services.AddSingleton<DownloadDirHistoryService>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<TorrentsViewModel>();
        services.AddSingleton<SearchViewModel>();
        services.AddSingleton<AddTorrentViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<StatusBarViewModel>();
        services.AddSingleton<PendingTorrentLaunchCoordinator>();
        return services;
    }
}
