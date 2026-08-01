using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class SearchViewModel : ViewModelBase
{
    private readonly HandlerInvoker _handlers;
    private readonly ITorrentProviderCatalog _catalog;
    private readonly LocalizationService _localization;
    private readonly AppToastService _toasts;
    private readonly NavigationService _navigation;
    private readonly AddTorrentViewModel _addTorrent;
    private readonly HashSet<string> _selectedIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _subscribedProviderIds = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _pageSubtitle = string.Empty;
    [ObservableProperty] private string _providersLabel = string.Empty;
    [ObservableProperty] private string _queryLabel = string.Empty;
    [ObservableProperty] private string _queryPlaceholder = string.Empty;
    [ObservableProperty] private string _searchButtonText = string.Empty;
    [ObservableProperty] private string _addProviderText = string.Empty;
    [ObservableProperty] private string _emptyText = string.Empty;
    [ObservableProperty] private string _columnName = string.Empty;
    [ObservableProperty] private string _columnSource = string.Empty;
    [ObservableProperty] private string _columnSize = string.Empty;
    [ObservableProperty] private string _columnLink = string.Empty;
    [ObservableProperty] private string _columnActions = string.Empty;
    [ObservableProperty] private string _downloadButtonText = string.Empty;
    [ObservableProperty] private string _openLinkButtonText = string.Empty;
    [ObservableProperty] private string _query = string.Empty;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasResults;
    [ObservableProperty] private bool _hasAvailableProviders;

    public ObservableCollection<SearchProviderTagViewModel> SelectedProviders { get; } = new();
    public ObservableCollection<SearchProviderOptionViewModel> AvailableProviders { get; } = new();
    public ObservableCollection<SearchResultRowViewModel> Results { get; } = new();

    public SearchViewModel(
        HandlerInvoker handlers,
        ITorrentProviderCatalog catalog,
        LocalizationService localization,
        AppToastService toasts,
        NavigationService navigation,
        AddTorrentViewModel addTorrent)
    {
        _handlers = handlers;
        _catalog = catalog;
        _localization = localization;
        _toasts = toasts;
        _navigation = navigation;
        _addTorrent = addTorrent;
        _localization.LanguageChanged += RefreshLabels;
        RefreshLabels();
        ReloadProviders();
    }

    public void ReloadProviders(bool notifyLoadErrors = false)
    {
        var knownIds = _catalog.GetProviders().Select(p => p.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _selectedIds.RemoveWhere(id => !knownIds.Contains(id));

        if (_selectedIds.Count == 0)
        {
            foreach (var provider in _catalog.GetProviders())
                _selectedIds.Add(provider.Id);
        }

        foreach (var provider in _catalog.GetProviders())
            SubscribeProviderResults(provider);

        RebuildProviderLists();
        RebuildResultsFromProviders();

        if (_catalog.LoadErrors.Count > 0)
        {
            StatusText = string.Join(" · ", _catalog.LoadErrors);
            if (notifyLoadErrors)
            {
                _toasts.ShowError(
                    _localization.T("searchPage.loadErrors"),
                    StatusText);
            }
        }

        RefreshEmptyState();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        ErrorMessage = string.Empty;
        var selected = SelectedProviders.Select(p => p.Id).ToList();
        if (selected.Count == 0)
        {
            ErrorMessage = _localization.T("searchPage.selectProvider");
            return;
        }

        if (string.IsNullOrWhiteSpace(Query))
        {
            ErrorMessage = _localization.T("searchPage.queryRequired");
            return;
        }

        IsBusy = true;
        try
        {
            if (!await EnsureProvidersLoggedInAsync(selected))
                return;
            RefreshProviderAuthStates();

            var result = await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<SearchAcrossProvidersHandler>().HandleAsync(
                    new ProviderSearchRequestDto(Query.Trim(), selected)));

            if (result.Errors.Count > 0
                && result.Errors.Any(IsSessionLostError)
                && selected.Any(id => _catalog.GetById(id) is { IsLoginRequired: true, IsLoggedIn: false }))
            {
                if (!await EnsureProvidersLoggedInAsync(selected))
                    return;
                result = await _handlers.InvokeAsync(sp =>
                    sp.GetRequiredService<SearchAcrossProvidersHandler>().HandleAsync(
                        new ProviderSearchRequestDto(Query.Trim(), selected)));
            }

            RebuildResultsFromProviders();

            if (result.Errors.Count > 0)
            {
                ErrorMessage = string.Join(" · ", result.Errors);
                Console.Error.WriteLine($"[Search] provider errors: {ErrorMessage}");
            }

            StatusText = _localization.Format(
                "searchPage.resultsCount",
                ("count", Results.Count.ToString()));
            RefreshEmptyState();
        }
        catch (Exception ex) when (IsSessionLostError(ex.Message))
        {
            Console.Error.WriteLine($"[Search] session lost, re-login: {ex.Message}");
            try
            {
                if (!await EnsureProvidersLoggedInAsync(selected))
                    return;

                var retry = await _handlers.InvokeAsync(sp =>
                    sp.GetRequiredService<SearchAcrossProvidersHandler>().HandleAsync(
                        new ProviderSearchRequestDto(Query.Trim(), selected)));
                RebuildResultsFromProviders();
                if (retry.Errors.Count > 0)
                {
                    ErrorMessage = string.Join(" · ", retry.Errors);
                    Console.Error.WriteLine($"[Search] provider errors: {ErrorMessage}");
                }

                StatusText = _localization.Format(
                    "searchPage.resultsCount",
                    ("count", Results.Count.ToString()));
                RefreshEmptyState();
            }
            catch (Exception retryEx)
            {
                Console.Error.WriteLine($"[Search] failed: {retryEx}");
                ErrorMessage = retryEx.Message;
                _toasts.ShowError(_localization.T("searchPage.searchFailed"), retryEx.Message);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Search] failed: {ex}");
            ErrorMessage = ex.Message;
            _toasts.ShowError(_localization.T("searchPage.searchFailed"), ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> EnsureProvidersLoggedInAsync(IReadOnlyList<string> selected)
    {
        foreach (var id in selected)
        {
            var provider = _catalog.GetById(id);
            if (provider is null)
                continue;

            SubscribeProviderResults(provider);

            if (provider.IsLoginRequired && !provider.IsLoggedIn)
            {
                await provider.LoginAsync();
                if (!provider.IsLoggedIn)
                {
                    ErrorMessage = _localization.Format(
                        "searchPage.loginRequired",
                        ("name", provider.DisplayName));
                    RefreshProviderAuthStates();
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsSessionLostError(string? message) =>
        !string.IsNullOrWhiteSpace(message)
        && (message.Contains("Cloudflare", StringComparison.OrdinalIgnoreCase)
            || message.Contains("session expired", StringComparison.OrdinalIgnoreCase)
            || message.Contains("login again", StringComparison.OrdinalIgnoreCase)
            || message.Contains("sign in again", StringComparison.OrdinalIgnoreCase));

    private void SubscribeProviderResults(ITorrentProvider provider)
    {
        if (!_subscribedProviderIds.Add(provider.Id))
            return;

        provider.Results.CollectionChanged += OnProviderResultsChanged;
    }

    private void OnProviderResultsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
            RebuildResultsFromProviders();
        else
            Dispatcher.UIThread.Post(RebuildResultsFromProviders);
    }

    private void RebuildResultsFromProviders()
    {
        Results.Clear();
        foreach (var tag in SelectedProviders)
        {
            var provider = _catalog.GetById(tag.Id);
            if (provider is null)
                continue;

            foreach (var hit in provider.Results)
            {
                Results.Add(new SearchResultRowViewModel(
                    provider.Id,
                    provider.DisplayName,
                    hit.Id,
                    hit.Title,
                    hit.SizeBytes,
                    hit.DetailUrl,
                    DownloadButtonText,
                    OpenLinkButtonText,
                    DownloadHitAsync,
                    OpenLinkAsync));
            }
        }

        HasResults = Results.Count > 0;
    }

    private async Task DownloadHitAsync(SearchResultRowViewModel row)
    {
        var provider = _catalog.GetById(row.ProviderId);
        if (provider is null)
        {
            _toasts.ShowError(_localization.T("searchPage.downloadFailed"), row.ProviderId);
            return;
        }

        row.IsBusy = true;
        try
        {
            if (provider.IsLoginRequired && !provider.IsLoggedIn)
            {
                await provider.LoginAsync();
                if (!provider.IsLoggedIn)
                    return;
            }

            var bytes = await provider.DownloadTorrentAsync(row.HitId);
            if (bytes.Length == 0)
                throw new InvalidOperationException("Empty torrent file.");

            var base64 = Convert.ToBase64String(bytes);
            _navigation.Navigate(AppPage.AddTorrent);
            await _addTorrent.OpenFromMetainfoBase64Async(base64, $"{row.Title}.torrent");
            _toasts.ShowSuccess(_localization.T("searchPage.downloadReady"));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Search] download error: {ex}");
            _toasts.ShowError(_localization.T("searchPage.downloadFailed"), ex.Message);
        }
        finally
        {
            row.IsBusy = false;
        }
    }

    private Task OpenLinkAsync(SearchResultRowViewModel row)
    {
        if (string.IsNullOrWhiteSpace(row.DetailUrl))
            return Task.CompletedTask;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = row.DetailUrl,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            _toasts.ShowError(_localization.T("searchPage.openLinkFailed"), ex.Message);
        }

        return Task.CompletedTask;
    }

    private void RebuildProviderLists()
    {
        SelectedProviders.Clear();
        AvailableProviders.Clear();

        foreach (var provider in _catalog.GetProviders())
        {
            if (_selectedIds.Contains(provider.Id))
            {
                SelectedProviders.Add(new SearchProviderTagViewModel(
                    provider,
                    _localization.T("searchPage.logout"),
                    RemoveProvider,
                    LogoutProviderAsync));
            }
            else
            {
                AvailableProviders.Add(new SearchProviderOptionViewModel(
                    provider.Id,
                    provider.DisplayName,
                    AddProvider));
            }
        }

        HasAvailableProviders = AvailableProviders.Count > 0;
    }

    private async Task LogoutProviderAsync(SearchProviderTagViewModel tag)
    {
        var provider = _catalog.GetById(tag.Id);
        if (provider is null)
            return;

        try
        {
            await provider.LogoutAsync();
            tag.RefreshAuthState();
            RebuildResultsFromProviders();
            _toasts.ShowSuccess(
                _localization.Format("searchPage.logoutOk", ("name", provider.DisplayName)));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Search] logout error: {ex}");
            _toasts.ShowError(_localization.T("searchPage.logoutFailed"), ex.Message);
        }
    }

    private void RefreshProviderAuthStates()
    {
        foreach (var tag in SelectedProviders)
            tag.RefreshAuthState();
    }

    private void AddProvider(SearchProviderOptionViewModel option)
    {
        if (!_selectedIds.Add(option.Id))
            return;
        RebuildProviderLists();
        RebuildResultsFromProviders();
    }

    private void RemoveProvider(SearchProviderTagViewModel tag)
    {
        if (!_selectedIds.Remove(tag.Id))
            return;
        RebuildProviderLists();
        RebuildResultsFromProviders();
    }

    private void RefreshEmptyState()
    {
        HasResults = Results.Count > 0;
        if (_catalog.GetProviders().Count == 0 && string.IsNullOrWhiteSpace(StatusText))
            StatusText = _localization.T("searchPage.noProviders");
    }

    private void RefreshLabels()
    {
        PageTitle = _localization.T("searchPage.title");
        PageSubtitle = _localization.T("searchPage.subtitle");
        ProvidersLabel = _localization.T("searchPage.providers");
        QueryLabel = _localization.T("searchPage.query");
        QueryPlaceholder = _localization.T("searchPage.queryPlaceholder");
        SearchButtonText = _localization.T("searchPage.search");
        AddProviderText = _localization.T("searchPage.addProvider");
        EmptyText = _localization.T("searchPage.empty");
        ColumnName = _localization.T("searchPage.columnName");
        ColumnSource = _localization.T("searchPage.columnSource");
        ColumnSize = _localization.T("searchPage.columnSize");
        ColumnLink = _localization.T("searchPage.columnLink");
        ColumnActions = _localization.T("searchPage.columnActions");
        DownloadButtonText = _localization.T("searchPage.download");
        OpenLinkButtonText = _localization.T("searchPage.openLink");
        var logout = _localization.T("searchPage.logout");
        foreach (var tag in SelectedProviders)
        {
            tag.SetLogoutText(logout);
            tag.RefreshAuthState();
        }
        RefreshEmptyState();
    }
}