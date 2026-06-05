using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Handlers;
using TransmissonNET.App.Avalonia.Desktop;
using TransmissonNET.App.Avalonia.Services;

namespace TransmissonNET.App.Avalonia.Views;

internal partial class TorrentAssociationPromptWindow : Window
{
    private readonly HandlerInvoker _handlers;
    private readonly AppToastService _toasts;
    private readonly LocalizationService _localization;
    private bool _closed;

    public TorrentAssociationPromptWindow(
        LocalizationService localization,
        HandlerInvoker handlers,
        AppToastService toasts)
    {
        _localization = localization;
        _handlers = handlers;
        _toasts = toasts;
        InitializeComponent();
        Title = localization.T("torrentAssociationPrompt.title");
        BodyText.Text = localization.T("torrentAssociationPrompt.body");
        YesButton.Content = localization.T("common.yes");
        NoButton.Content = localization.T("common.no");
        WindowEscClose.Attach(this, () => _ = DeclineAsync());
    }

    private async void OnYes(object? sender, RoutedEventArgs e)
    {
        if (_closed)
            return;

        try
        {
            await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<RegisterTorrentFileAssociationHandler>().HandleAsync());
        }
        catch (Exception ex)
        {
            _toasts.ShowError(_localization.T("torrentAssociationPrompt.saveFailed"), ex.Message);
            return;
        }

        CloseOnce();
    }

    private async void OnNo(object? sender, RoutedEventArgs e)
    {
        await DeclineAsync();
        CloseOnce();
    }

    private async Task DeclineAsync()
    {
        if (_closed)
            return;

        try
        {
            await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<DeclineTorrentFileAssociationHandler>().HandleAsync());
        }
        catch (Exception ex)
        {
            _toasts.ShowError(_localization.T("torrentAssociationPrompt.saveFailed"), ex.Message);
        }
    }

    private void CloseOnce()
    {
        if (_closed)
            return;

        _closed = true;
        Close();
    }
}
