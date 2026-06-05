using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.App.Avalonia.Features.TorrentStatus;
using TransmissonNET.App.Avalonia.Services;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class StatusBarViewModel : ViewModelBase
{
    private readonly HandlerInvoker _handlers;
    private readonly LocalizationService _localization;

    [ObservableProperty]
    private string _daemonStatus = string.Empty;

    [ObservableProperty]
    private bool _isDaemonConnected;

    [ObservableProperty]
    private long _downloadSpeed;

    [ObservableProperty]
    private long _uploadSpeed;

    [ObservableProperty]
    private int _downloadingCount;

    [ObservableProperty]
    private int _completedCount;

    public string DownloadingLabel => _localization.T("statusBar.downloading") + ": ";
    public string CompletedLabel => _localization.T("statusBar.completed") + ": ";

    public StatusBarViewModel(HandlerInvoker handlers, LocalizationService localization)
    {
        _handlers = handlers;
        _localization = localization;
        _localization.LanguageChanged += RefreshLabels;
        RefreshLabels();
    }

    public void ApplyTorrentMetrics(IEnumerable<TorrentDto> torrents)
    {
        var metrics = TorrentStatusMetrics.Derive(torrents);
        DownloadSpeed = metrics.DownloadSpeed;
        UploadSpeed = metrics.UploadSpeed;
        DownloadingCount = metrics.Downloading;
        CompletedCount = metrics.Completed;
        IsDaemonConnected = true;
        DaemonStatus = _localization.T("statusBar.daemonOnline");
    }

    public void ApplyDaemonOffline()
    {
        IsDaemonConnected = false;
        DaemonStatus = _localization.T("statusBar.daemonOffline");
        DownloadSpeed = 0;
        UploadSpeed = 0;
        DownloadingCount = 0;
        CompletedCount = 0;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _handlers.InvokeAsync(
                sp => sp.GetRequiredService<GetDaemonStatusHandler>().HandleAsync(true, cancellationToken),
                cancellationToken);

            IsDaemonConnected = status.Connected;
            DaemonStatus = status.Connected
                ? _localization.T("statusBar.daemonOnline")
                : _localization.T("statusBar.daemonOffline");
            DownloadSpeed = status.DownloadSpeed;
            UploadSpeed = status.UploadSpeed;
            DownloadingCount = status.DownloadingCount;
            CompletedCount = status.CompletedCount;
        }
        catch
        {
            ApplyDaemonOffline();
        }
    }

    private void RefreshLabels()
    {
        OnPropertyChanged(nameof(DaemonStatus));
        OnPropertyChanged(nameof(DownloadingLabel));
        OnPropertyChanged(nameof(CompletedLabel));
    }
}
