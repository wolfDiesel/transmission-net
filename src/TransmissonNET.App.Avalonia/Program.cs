using System.Text;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.Desktop;

namespace TransmissonNET.App.Avalonia;

sealed class Program
{
    public static string? PendingTorrentPath { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        var launchTorrentPath = CommandLineTorrentLaunch.FindTorrentPath(args);
        if (SingleInstanceHost.TryForwardToRunningInstance(launchTorrentPath))
            return;

        PendingTorrentPath = launchTorrentPath;

        var services = new ServiceCollection();
        services.AddAvaloniaApp();
        AppServices.Initialize(services.BuildServiceProvider());

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
