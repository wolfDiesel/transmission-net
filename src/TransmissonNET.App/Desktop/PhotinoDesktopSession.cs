using Photino.NET;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Domain;

namespace TransmissonNET.App.Desktop;

internal sealed class PhotinoDesktopSession : IAsyncDisposable
{
    private readonly string _baseUrl;
    private readonly ISettingsStore _settingsStore;

    private AppSettings _settings = null!;
    private PhotinoWindow? _window;
    private ILinuxTrayHost? _tray;
    private bool _quitRequested;

    public PhotinoDesktopSession(string baseUrl, ISettingsStore settingsStore)
    {
        _baseUrl = baseUrl;
        _settingsStore = settingsStore;
    }

    public async Task RunAsync()
    {
        _settings = await _settingsStore.LoadAsync().ConfigureAwait(false);

        var window = new PhotinoWindow()
            .SetTitle(GtkWindowControl.WindowTitle)
            .SetUseOsDefaultSize(false)
            .SetSize(_settings.Ui.WindowWidth, _settings.Ui.WindowHeight)
            .Center();

        window.RegisterWindowCreatedHandler((_, _) =>
        {
            GtkDeleteEventHook.Install(ShouldCloseToTray);
            _ = StartTraySafeAsync();
        });

        _window = window;
        window.Load(_baseUrl);
        window.WaitForClose();
    }

    public ValueTask DisposeAsync()
    {
        _tray?.Dispose();
        _tray = null;
        return ValueTask.CompletedTask;
    }

    public void ShowMainWindow()
    {
        GtkWindowControl.TryShow();
        DesktopWindowActivator.TryActivate();
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

    private void RequestQuit()
    {
        _quitRequested = true;
        GtkDeleteEventHook.AllowClose();
        _tray?.Dispose();
        _tray = null;
        _window?.Close();
    }
}
