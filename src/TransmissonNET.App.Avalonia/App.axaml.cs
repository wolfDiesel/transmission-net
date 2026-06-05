using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.App.Avalonia.ViewModels;
using TransmissonNET.App.Avalonia.Views;
using TransmissonNET.Desktop;

namespace TransmissonNET.App.Avalonia;

public partial class App : global::Avalonia.Application
{
    private AvaloniaDesktopSession? _desktopSession;
    private SingleInstanceHost? _singleInstance;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        var mainVm = AppServices.GetRequired<MainWindowViewModel>();
        var settingsStore = AppServices.CreateScope().ServiceProvider.GetRequiredService<ISettingsStore>();
        var settings = await settingsStore.LoadAsync();
        AppServices.GetRequired<ThemeService>().Apply(
            new TransmissonNET.Application.Contracts.UiSettingsDto(
                settings.Ui.RefreshIntervalSeconds,
                settings.Ui.WindowWidth,
                settings.Ui.WindowHeight,
                new TransmissonNET.Application.Contracts.TorrentTableSettingsDto([], "name", false),
                settings.Ui.ColorScheme,
                settings.Ui.Appearance,
                settings.Ui.DownloadDirHistory,
                settings.Ui.TorrentFileAssociation,
                settings.Ui.TrayEnabled,
                settings.Ui.MinimizeToTray,
                settings.Ui.CloseToTray,
                settings.Ui.Language));

        var window = new MainWindow
        {
            DataContext = mainVm,
            Width = settings.Ui.WindowWidth,
            Height = settings.Ui.WindowHeight,
            Title = GtkWindowControl.WindowTitle,
        };

        _desktopSession = new AvaloniaDesktopSession(settingsStore, () => window);
        await _desktopSession.InitializeAsync();

        var pendingStore = AppServices.CreateScope().ServiceProvider.GetRequiredService<IPendingTorrentLaunchStore>();
        _singleInstance = new SingleInstanceHost(pendingStore, () => _desktopSession.ShowMainWindow());
        _singleInstance.Start();

        if (!string.IsNullOrEmpty(Program.PendingTorrentPath))
            pendingStore.SetPendingPath(Program.PendingTorrentPath);

        window.Closing += (_, e) =>
        {
            if (_desktopSession.TryCancelClose(window))
                e.Cancel = true;
        };

        var pendingLaunch = AppServices.GetRequired<PendingTorrentLaunchCoordinator>();
        pendingLaunch.Configure(() => _desktopSession.ShowMainWindow());
        pendingLaunch.Start();

        desktop.MainWindow = window;
        desktop.ShutdownRequested += async (_, _) =>
        {
            pendingLaunch.Dispose();
            if (_singleInstance is not null)
                await _singleInstance.DisposeAsync();
            if (_desktopSession is not null)
                await _desktopSession.DisposeAsync();
        };

        await mainVm.InitializeAsync();
        window.Show();
        await pendingLaunch.ProcessStartupAsync(window);

        base.OnFrameworkInitializationCompleted();
    }
}
