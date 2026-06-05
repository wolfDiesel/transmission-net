using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Desktop;
using TransmissonNET.Domain;

namespace TransmissonNET.App.Avalonia.Services;

internal sealed class AvaloniaDesktopSession : IAsyncDisposable
{
    private readonly ISettingsStore _settingsStore;
    private readonly Func<Window?> _windowProvider;

    private AppSettings _settings = null!;
    private ILinuxTrayHost? _tray;
    private bool _quitRequested;

    public AvaloniaDesktopSession(ISettingsStore settingsStore, Func<Window?> windowProvider)
    {
        _settingsStore = settingsStore;
        _windowProvider = windowProvider;
    }

    public async Task InitializeAsync()
    {
        _settings = await _settingsStore.LoadAsync().ConfigureAwait(false);
        if (OperatingSystem.IsLinux())
            GtkDeleteEventHook.Install(ShouldCloseToTray);
        await StartTraySafeAsync().ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        _tray?.Dispose();
        _tray = null;
        return ValueTask.CompletedTask;
    }

    public bool TryCancelClose(Window window)
    {
        if (!ShouldCloseToTray())
            return false;

        window.Hide();
        return true;
    }

    public void ShowMainWindow() => PostToUiThread(ShowMainWindowCore);

    public void RequestQuit() => PostToUiThread(RequestQuitCore);

    private void ShowMainWindowCore()
    {
        var window = _windowProvider();
        if (window is null)
            return;

        window.Show();
        window.WindowState = WindowState.Normal;
        window.Activate();
        GtkWindowControl.TryShow();
        DesktopWindowActivator.TryActivate();
    }

    private void RequestQuitCore()
    {
        _quitRequested = true;
        GtkDeleteEventHook.AllowClose();
        _tray?.Dispose();
        _tray = null;

        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    private static void PostToUiThread(Action action)
    {
        var dispatcher = Dispatcher.UIThread;
        if (dispatcher.CheckAccess())
            action();
        else
            dispatcher.Post(action);
    }

    private bool ShouldCloseToTray() =>
        OperatingSystem.IsLinux()
        && _settings.Ui.TrayEnabled
        && _settings.Ui.CloseToTray
        && _tray?.IsActive == true
        && !_quitRequested;

    private async Task StartTraySafeAsync()
    {
        if (!OperatingSystem.IsLinux() || !_settings.Ui.TrayEnabled)
            return;

        try
        {
            var tray = LinuxTrayHost.TryCreate(TrayIconPaths.ResolveTrayIconPath());
            if (tray is null)
                return;

            tray.ShowRequested += ShowMainWindow;
            tray.QuitRequested += RequestQuit;
            await tray.StartAsync().ConfigureAwait(false);

            if (!tray.IsActive)
            {
                tray.Dispose();
                return;
            }

            _tray = tray;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"System tray: {ex.Message}");
        }
    }
}
