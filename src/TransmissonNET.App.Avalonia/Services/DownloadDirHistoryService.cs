using System.Collections.ObjectModel;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.App.Avalonia.Services;

internal sealed class DownloadDirHistoryService
{
    private readonly HandlerInvoker _handlers;
    private readonly DispatcherTimer _saveTimer;
    private AppSettingsDto? _settings;
    private bool _loaded;

    public DownloadDirHistoryService(HandlerInvoker handlers)
    {
        _handlers = handlers;
        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _saveTimer.Tick += async (_, _) => await FlushSaveAsync();
    }

    public ObservableCollection<string> Directories { get; } = new();

    public async Task LoadAsync()
    {
        if (_loaded)
            return;

        _settings = await _handlers.InvokeAsync(sp =>
            sp.GetRequiredService<GetSettingsHandler>().HandleAsync());
        ReplaceDirectories(_settings.Ui.DownloadDirHistory);
        _loaded = true;
    }

    public string ResolveDefault(string? sessionDownloadDir)
    {
        if (Directories.Count > 0 && !string.IsNullOrWhiteSpace(Directories[0]))
            return Directories[0];

        return sessionDownloadDir?.Trim() ?? string.Empty;
    }

    public void Remember(string path)
    {
        var trimmed = path.Trim();
        if (trimmed.Length == 0)
            return;

        if (Directories.Count > 0 && string.Equals(Directories[0], trimmed, StringComparison.Ordinal))
            return;

        ReplaceDirectories(DownloadDirHistoryHelper.Remember(Directories.ToList(), trimmed));
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void ReplaceDirectories(IReadOnlyList<string>? paths)
    {
        Directories.Clear();
        if (paths is null)
            return;

        foreach (var path in paths)
        {
            if (!string.IsNullOrWhiteSpace(path))
                Directories.Add(path);
        }
    }

    private async Task FlushSaveAsync()
    {
        _saveTimer.Stop();
        if (_settings is null)
            return;

        var ui = _settings.Ui with { DownloadDirHistory = Directories.ToList() };
        _settings = await _handlers.InvokeAsync(sp =>
            sp.GetRequiredService<SaveSettingsHandler>().HandleAsync(_settings with { Ui = ui }));
    }
}
