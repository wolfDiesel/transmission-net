using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Handlers;
using TransmissonNET.App.Avalonia.ViewModels;
using TransmissonNET.App.Avalonia.Views;

namespace TransmissonNET.App.Avalonia.Services;

internal sealed class PendingTorrentLaunchCoordinator : IDisposable
{
    private readonly HandlerInvoker _handlers;
    private readonly NavigationService _navigation;
    private readonly AddTorrentViewModel _addTorrent;
    private readonly LocalizationService _localization;
    private readonly AppToastService _toasts;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private Action? _showMainWindow;
    private int _handling;

    public PendingTorrentLaunchCoordinator(
        HandlerInvoker handlers,
        NavigationService navigation,
        AddTorrentViewModel addTorrent,
        LocalizationService localization,
        AppToastService toasts)
    {
        _handlers = handlers;
        _navigation = navigation;
        _addTorrent = addTorrent;
        _localization = localization;
        _toasts = toasts;
        _timer.Tick += async (_, _) => await TryConsumePendingAsync();
    }

    public void Configure(Action showMainWindow) => _showMainWindow = showMainWindow;

    public void Start() => _timer.Start();

    public async Task ProcessStartupAsync(Window owner)
    {
        await TryConsumePendingAsync();
        await TryShowAssociationPromptAsync(owner);
    }

    private async Task TryConsumePendingAsync()
    {
        if (Interlocked.CompareExchange(ref _handling, 1, 0) != 0)
            return;

        try
        {
            var path = await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<GetPendingTorrentLaunchPathHandler>().HandleAsync(consume: true));
            if (string.IsNullOrWhiteSpace(path))
                return;

            _showMainWindow?.Invoke();
            if (_navigation.CurrentPage != AppPage.AddTorrent)
                _navigation.Navigate(AppPage.AddTorrent);

            await _addTorrent.OpenFromPathAsync(path);
        }
        finally
        {
            Interlocked.Exchange(ref _handling, 0);
        }
    }

    private async Task TryShowAssociationPromptAsync(Window owner)
    {
        var status = await _handlers.InvokeAsync(sp =>
            sp.GetRequiredService<GetTorrentFileAssociationStatusHandler>().HandleAsync());
        if (!status.ShouldPrompt)
            return;

        var dialog = new TorrentAssociationPromptWindow(_localization, _handlers, _toasts);
        await dialog.ShowDialog(owner);
    }

    public void Dispose() => _timer.Stop();
}
