using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Application.Settings;
using TransmissonNET.App.Avalonia.Features.TorrentTable;
using TransmissonNET.Application.Torrents;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.App.Avalonia.Views;
using TransmissonNET.Domain;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class TorrentsViewModel : ViewModelBase
{
    private readonly HandlerInvoker _handlers;
    private readonly LocalizationService _localization;
    private readonly StatusBarViewModel _statusBar;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(3) };
    private readonly DispatcherTimer _saveTimer = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private int _refreshSeconds = 3;
    private bool _pollingPaused;
    private bool _isRefreshing;
    private bool _initialLoad = true;
    private AppSettingsDto? _appSettings;
    private bool _savePending;

    private readonly ObservableCollection<TorrentRowViewModel> _allTorrents = new();

    [ObservableProperty]
    private ObservableCollection<TorrentRowViewModel> _torrents = new();

    [ObservableProperty]
    private string _nameFilterQuery = string.Empty;

    [ObservableProperty]
    private TorrentRowViewModel? _selectedTorrent;

    private IReadOnlyList<TorrentRowViewModel> _selectedTorrents = Array.Empty<TorrentRowViewModel>();

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _pageTitle = string.Empty;

    [ObservableProperty]
    private string _columnsButtonText = string.Empty;

    [ObservableProperty]
    private string _columnsPanelTitle = string.Empty;

    [ObservableProperty]
    private string _columnsPanelHint = string.Empty;

    [ObservableProperty]
    private string _detailsButtonText = string.Empty;

    [ObservableProperty]
    private string _refreshButtonText = string.Empty;

    [ObservableProperty]
    private string _nameFilterPlaceholder = string.Empty;

    [ObservableProperty]
    private string _clearFilterButtonText = string.Empty;

    public bool HasNameFilter => !string.IsNullOrWhiteSpace(NameFilterQuery);

    [ObservableProperty]
    private string _contextMenuStart = string.Empty;

    [ObservableProperty]
    private string _contextMenuStop = string.Empty;

    [ObservableProperty]
    private string _contextMenuVerify = string.Empty;

    [ObservableProperty]
    private string _contextMenuHigh = string.Empty;

    [ObservableProperty]
    private string _contextMenuNormal = string.Empty;

    [ObservableProperty]
    private string _contextMenuLow = string.Empty;

    [ObservableProperty]
    private string _contextMenuMove = string.Empty;

    [ObservableProperty]
    private string _contextMenuRemove = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TorrentColumnItemViewModel> _tableColumns = new();

    public event Action? TableLayoutChanged;

    public event Action? SortStateChanged;

    public string SortColumnId { get; private set; } = TorrentTableColumnIds.Name;

    public bool SortDescending { get; private set; }

    public TorrentsViewModel(
        HandlerInvoker handlers,
        LocalizationService localization,
        StatusBarViewModel statusBar)
    {
        _handlers = handlers;
        _localization = localization;
        _statusBar = statusBar;
        _timer.Tick += async (_, _) => await RefreshAsync();
        _saveTimer.Tick += async (_, _) => await FlushTableSettingsAsync();
        _localization.LanguageChanged += OnLanguageChanged;
        RefreshLocalizedStrings();
    }

    public async Task InitializeAsync()
    {
        var settings = await _handlers.InvokeAsync(sp => sp.GetRequiredService<GetSettingsHandler>().HandleAsync());
        _appSettings = settings;
        _refreshSeconds = Math.Max(1, settings.Ui.RefreshIntervalSeconds);
        _timer.Interval = TimeSpan.FromSeconds(_refreshSeconds);
        ApplyTableSettings(settings.Ui.TorrentTable);
        _timer.Start();
        await RefreshAsync();
    }

    public void SetPollingPaused(bool paused) => _pollingPaused = paused;

    public void SetSelectedTorrents(IReadOnlyList<TorrentRowViewModel> selected) =>
        _selectedTorrents = selected;

    public IReadOnlyList<TorrentTableColumnSettingDto> GetVisibleColumnsInOrder() =>
        TableColumns
            .Where(column => column.Visible)
            .Select(column => new TorrentTableColumnSettingDto(
                column.Id,
                true,
                ResolveColumnWidth(column.Id)))
            .ToList();

    public string GetColumnLabel(string columnId) => _localization.T($"torrentTable.columns.{columnId}");

    public string GetSortMark(string columnId)
    {
        if (SortColumnId != columnId)
            return string.Empty;

        return SortDescending ? " ↓" : " ↑";
    }

    public void MoveColumn(string fromId, string toId)
    {
        if (fromId == toId)
            return;

        var fromIndex = IndexOfColumn(fromId);
        var toIndex = IndexOfColumn(toId);
        if (fromIndex < 0 || toIndex < 0)
            return;

        TableColumns.Move(fromIndex, toIndex);
        QueueTableSettingsSave();
        TableLayoutChanged?.Invoke();
    }

    public void ToggleSort(string columnId)
    {
        var column = TorrentTableColumns.Find(columnId);
        if (column is null || !column.Sortable)
            return;

        if (SortColumnId == columnId)
            SortDescending = !SortDescending;
        else
        {
            SortColumnId = columnId;
            SortDescending = false;
        }

        ApplySort();
        QueueTableSettingsSave();
        SortStateChanged?.Invoke();
    }

    partial void OnNameFilterQueryChanged(string value)
    {
        ReapplyNameFilter();
        OnPropertyChanged(nameof(HasNameFilter));
    }

    [RelayCommand]
    private void ClearNameFilter()
    {
        NameFilterQuery = string.Empty;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (_pollingPaused || _isRefreshing)
            return;

        _isRefreshing = true;
        if (_initialLoad)
            IsLoading = true;

        try
        {
            var torrents = await _handlers.InvokeAsync(async sp =>
            {
                var items = await sp.GetRequiredService<GetTorrentsHandler>().HandleAsync();
                return TorrentDtoMapper.ToDtoList(items);
            });

            MergeTorrents(torrents);
            _statusBar.ApplyTorrentMetrics(torrents);
            ErrorMessage = null;
            StatusText = _localization.Format(
                "torrentsPage.updated",
                ("time", DateTime.Now.ToString("t")),
                ("seconds", _refreshSeconds.ToString()));
        }
        catch (Exception ex)
        {
            _statusBar.ApplyDaemonOffline();
            ErrorMessage = ex.Message;
        }
        finally
        {
            _isRefreshing = false;
            if (_initialLoad)
            {
                IsLoading = false;
                _initialLoad = false;
            }
        }
    }

    private void MergeTorrents(IReadOnlyList<TorrentDto> incoming)
    {
        var existingById = _allTorrents.ToDictionary(row => row.Id);

        foreach (var dto in incoming)
        {
            if (existingById.Remove(dto.Id, out var row))
                row.UpdateFrom(dto);
            else
                _allTorrents.Add(TorrentRowViewModel.FromDto(dto));
        }

        foreach (var removed in existingById.Values)
            _allTorrents.Remove(removed);

        ApplySort();
    }

    private void ApplySort()
    {
        TorrentTableSorter.SortInPlace(_allTorrents, SortColumnId, SortDescending);
        ReapplyNameFilter();
    }

    private void ReapplyNameFilter()
    {
        var selectedId = SelectedTorrent?.Id;
        var visible = _allTorrents
            .Where(row => TorrentNameWildcardFilter.IsMatch(row.Name, NameFilterQuery))
            .ToList();

        TorrentTableSorter.SyncRowOrder(Torrents, visible);
        RestoreSelectedTorrent(selectedId);
    }

    private void RestoreSelectedTorrent(int? selectedId)
    {
        if (selectedId is not int id)
            return;

        if (SelectedTorrent?.Id == id)
            return;

        SelectedTorrent = Torrents.FirstOrDefault(row => row.Id == id);
    }

    private void ApplyTableSettings(TorrentTableSettingsDto? tableSettings)
    {
        IReadOnlyList<TorrentTableColumnSettingDto> columns;
        if (tableSettings?.Columns is { Count: > 0 } savedColumns)
        {
            columns = savedColumns;
            SortColumnId = TorrentTableColumnIds.All.Contains(tableSettings.SortColumnId)
                ? tableSettings.SortColumnId
                : TorrentTableColumnIds.Name;
            SortDescending = tableSettings.SortDescending;
        }
        else
        {
            var defaults = TorrentTableSettings.CreateDefault();
            columns = defaults.Columns
                .Select(column => new TorrentTableColumnSettingDto(column.Id, column.Visible, column.WidthPx))
                .ToList();
            SortColumnId = defaults.SortColumnId;
            SortDescending = defaults.SortDescending;
        }

        var labelsById = columns.ToDictionary(column => column.Id, column => column);
        var orderedItems = new List<TorrentColumnItemViewModel>();

        foreach (var column in columns)
        {
            if (TorrentTableColumns.Find(column.Id) is null)
                continue;

            orderedItems.Add(CreateColumnItem(column.Id, column.Visible));
        }

        foreach (var fallback in TorrentTableColumns.All)
        {
            if (orderedItems.Any(item => item.Id == fallback.Id))
                continue;

            var visible = labelsById.TryGetValue(fallback.Id, out var setting) && setting.Visible;
            orderedItems.Add(CreateColumnItem(fallback.Id, visible));
        }

        TableColumns = new ObservableCollection<TorrentColumnItemViewModel>(orderedItems);
        TableLayoutChanged?.Invoke();
    }

    private TorrentColumnItemViewModel CreateColumnItem(string id, bool visible)
    {
        var item = new TorrentColumnItemViewModel
        {
            Id = id,
            Label = GetColumnLabel(id),
            Visible = visible,
        };
        item.VisibilityChanged = OnColumnVisibilityChanged;
        return item;
    }

    private void OnColumnVisibilityChanged(string columnId, bool visible)
    {
        QueueTableSettingsSave();
        TableLayoutChanged?.Invoke();
    }

    private int? ResolveColumnWidth(string columnId)
    {
        var stored = _appSettings?.Ui.TorrentTable.Columns.FirstOrDefault(column => column.Id == columnId)?.WidthPx;
        if (stored is not null)
            return stored;

        return TorrentTableColumns.Find(columnId)?.DefaultWidthPx;
    }

    private int IndexOfColumn(string columnId)
    {
        for (var index = 0; index < TableColumns.Count; index++)
        {
            if (TableColumns[index].Id == columnId)
                return index;
        }

        return -1;
    }

    private void QueueTableSettingsSave()
    {
        _savePending = true;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private async Task FlushTableSettingsAsync()
    {
        _saveTimer.Stop();
        if (!_savePending || _appSettings is null)
            return;

        _savePending = false;

        try
        {
            var table = BuildTableSettingsDto();
            var next = _appSettings with
            {
                Ui = _appSettings.Ui with { TorrentTable = table },
            };

            _appSettings = await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<SaveSettingsHandler>().HandleAsync(next));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private TorrentTableSettingsDto BuildTableSettingsDto() =>
        new(
            TableColumns
                .Select(column => new TorrentTableColumnSettingDto(
                    column.Id,
                    column.Visible,
                    ResolveColumnWidth(column.Id)))
                .ToList(),
            SortColumnId,
            SortDescending);

    private void OnLanguageChanged()
    {
        RefreshLocalizedStrings();
        foreach (var column in TableColumns)
            column.Label = GetColumnLabel(column.Id);

        OnPropertyChanged(nameof(StatusText));
        TableLayoutChanged?.Invoke();
    }

    private void RefreshLocalizedStrings()
    {
        PageTitle = _localization.T("torrentsPage.title");
        ColumnsButtonText = _localization.T("torrentsPage.columns");
        ColumnsPanelTitle = _localization.T("torrentsPage.columnsPanelTitle");
        ColumnsPanelHint = _localization.T("torrentsPage.columnsPanelHint");
        DetailsButtonText = _localization.T("torrentsPage.details");
        RefreshButtonText = _localization.T("common.refresh");
        NameFilterPlaceholder = _localization.T("torrentsPage.nameFilter");
        ClearFilterButtonText = _localization.T("torrentsPage.clearFilter");
        ContextMenuStart = _localization.T("torrentTable.contextMenu.start");
        ContextMenuStop = _localization.T("torrentTable.contextMenu.stop");
        ContextMenuVerify = _localization.T("torrentTable.contextMenu.verify");
        ContextMenuHigh = _localization.T("torrentTable.contextMenu.high");
        ContextMenuNormal = _localization.T("torrentTable.contextMenu.normal");
        ContextMenuLow = _localization.T("torrentTable.contextMenu.low");
        ContextMenuMove = _localization.T("torrentTable.contextMenu.move");
        ContextMenuRemove = _localization.T("torrentTable.contextMenu.remove");
    }

    [RelayCommand]
    private async Task StartSelectedAsync()
    {
        var ids = GetSelectedIds();
        if (ids.Count == 0)
            return;
        await ExecuteActionAsync("start", ids);
    }

    [RelayCommand]
    private async Task StopSelectedAsync()
    {
        var ids = GetSelectedIds();
        if (ids.Count == 0)
            return;
        await ExecuteActionAsync("stop", ids);
    }

    [RelayCommand]
    private async Task VerifySelectedAsync()
    {
        var ids = GetSelectedIds();
        if (ids.Count == 0)
            return;
        await ExecuteActionAsync("verify", ids);
    }

    [RelayCommand]
    private async Task SetPriorityAsync(string priority)
    {
        var ids = GetSelectedIds();
        if (ids.Count == 0)
            return;
        await ExecuteActionAsync("set-priority", ids, priority: priority);
    }

    [RelayCommand]
    private async Task RemoveSelectedAsync()
    {
        var targets = GetSelectedTorrents();
        if (targets.Count == 0)
            return;

        var prompt = targets.Count == 1
            ? $"{_localization.T("torrentTable.removeDialog.aboutToRemove")} {targets[0].Name}"
            : BuildMultiRemovePrompt(targets);
        var dialog = new RemoveTorrentDialog(prompt);
        var owner = GetOwnerWindow();
        if (owner is not null)
            await dialog.ShowDialog(owner);

        if (!dialog.Confirmed)
            return;

        await ExecuteActionAsync("remove", targets.Select(t => t.Id).ToList(), deleteLocalData: dialog.DeleteLocalData);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task MoveSelectedAsync()
    {
        var targets = GetSelectedTorrents();
        if (targets.Count == 0)
            return;

        var dialog = new MoveTorrentDialog(targets[0].DownloadDir);
        var owner = GetOwnerWindow();
        if (owner is not null)
            await dialog.ShowDialog(owner);

        if (!dialog.Confirmed || string.IsNullOrWhiteSpace(dialog.Destination))
            return;

        await ExecuteActionAsync(
            "move",
            targets.Select(t => t.Id).ToList(),
            location: dialog.Destination,
            move: dialog.MoveData);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task OpenDetailsAsync()
    {
        if (SelectedTorrent is null)
            return;

        SetPollingPaused(true);
        try
        {
            var window = new TorrentDetailsWindow(SelectedTorrent.Id, SelectedTorrent.Name);
            var owner = GetOwnerWindow();
            if (owner is not null)
                await window.ShowDialog(owner);
            else
                window.Show();
        }
        finally
        {
            SetPollingPaused(false);
            await RefreshAsync();
        }
    }

    private IReadOnlyList<TorrentRowViewModel> GetSelectedTorrents() =>
        _selectedTorrents.Count > 0
            ? _selectedTorrents
            : SelectedTorrent is not null
                ? [SelectedTorrent]
                : Array.Empty<TorrentRowViewModel>();

    private IReadOnlyList<int> GetSelectedIds() =>
        GetSelectedTorrents().Select(t => t.Id).ToList();

    private string BuildMultiRemovePrompt(IReadOnlyList<TorrentRowViewModel> targets)
    {
        var header = _localization.Format(
            "torrentTable.removeDialog.aboutToRemoveMultiple",
            ("count", targets.Count.ToString()));
        var names = string.Join(
            Environment.NewLine,
            targets.Take(8).Select(t => $"• {t.Name}"));
        if (targets.Count > 8)
            names += Environment.NewLine + _localization.Format(
                "torrentTable.removeDialog.andMore",
                ("count", (targets.Count - 8).ToString()));
        return $"{header}{Environment.NewLine}{names}";
    }

    private async Task ExecuteActionAsync(
        string action,
        IReadOnlyList<int> ids,
        string? priority = null,
        string? location = null,
        bool move = false,
        bool deleteLocalData = false)
    {
        try
        {
            var dto = new TorrentActionDto(action, ids, deleteLocalData, priority, location, move);
            await _handlers.InvokeAsync(sp => sp.GetRequiredService<ExecuteTorrentActionHandler>().HandleAsync(dto));
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private static global::Avalonia.Controls.Window? GetOwnerWindow() =>
        global::Avalonia.Application.Current?.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
}
