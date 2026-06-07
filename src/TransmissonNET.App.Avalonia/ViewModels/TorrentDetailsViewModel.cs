using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Application.Settings;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.Domain;
using TransmissonNET.App.Avalonia.Views;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class TorrentDetailsViewModel : ViewModelBase, IDisposable
{
    private readonly HandlerInvoker _handlers;
    private readonly LocalizationService _localization;
    private readonly int _torrentId;
    private readonly DispatcherTimer _filePollTimer;
    private readonly EventHandler _filePollTickHandler;
    private bool _filesLoaded;
    private bool _filesTabActive;
    private bool _disposed;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _status = string.Empty;
    [ObservableProperty] private string _progress = string.Empty;
    [ObservableProperty] private string _downloadDir = string.Empty;
    [ObservableProperty] private string _hash = string.Empty;
    [ObservableProperty] private string _comment = string.Empty;
    [ObservableProperty] private string _transferSummary = string.Empty;
    [ObservableProperty] private ObservableCollection<TorrentFileNodeItemViewModel> _fileTree = new();
    [ObservableProperty] private TorrentFileNodeItemViewModel? _selectedFileNode;
    [ObservableProperty] private string? _errorMessage;

    private IReadOnlyList<TorrentFileNodeItemViewModel> _selectedFileNodes = Array.Empty<TorrentFileNodeItemViewModel>();

    public event Action<IReadOnlyList<string>>? FileTreeSelectionRestoreRequested;

    public bool CanRenameFile =>
        GetSelectedFileNodes().Count == 1 && GetSelectedFileNodes()[0] is { IsFolder: false };

    public bool CanMassRenameFolder =>
        GetSelectedFileNodes().Count == 1 && GetSelectedFileNodes()[0].IsFolder;

    public bool CanSetFilePriority => GetSelectedFileIndices().Count > 0;
    [ObservableProperty] private string _tabGeneral = string.Empty;
    [ObservableProperty] private string _tabTransfer = string.Empty;
    [ObservableProperty] private string _tabFiles = string.Empty;
    [ObservableProperty] private string _massRenameAllLabel = string.Empty;
    [ObservableProperty] private string _renameFileLabel = string.Empty;
    [ObservableProperty] private string _massRenameFolderLabel = string.Empty;
    [ObservableProperty] private string _filePriorityLabel = string.Empty;
    [ObservableProperty] private string _priorityHighLabel = string.Empty;
    [ObservableProperty] private string _priorityNormalLabel = string.Empty;
    [ObservableProperty] private string _priorityLowLabel = string.Empty;
    [ObservableProperty] private string _fileColumnName = string.Empty;
    [ObservableProperty] private string _fileColumnPriority = string.Empty;
    [ObservableProperty] private string _fileColumnProgress = string.Empty;

    public TorrentDetailsViewModel(HandlerInvoker handlers, LocalizationService localization, int torrentId, string title)
    {
        _handlers = handlers;
        _localization = localization;
        _torrentId = torrentId;
        Name = title;
        _filePollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _filePollTickHandler = async (_, _) => await OnFilePollTickAsync();
        _filePollTimer.Tick += _filePollTickHandler;
        _localization.LanguageChanged += RefreshLabels;
        RefreshLabels();
    }

    public async Task LoadSummaryAsync()
    {
        if (_disposed)
            return;

        try
        {
            var details = await FetchDetailsAsync();
            if (details is null)
            {
                ErrorMessage = "Torrent not found";
                return;
            }

            ApplySummary(details);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public async void SetFilesTabActive(bool active)
    {
        if (_disposed)
            return;

        if (_filesTabActive == active)
            return;

        _filesTabActive = active;
        if (active)
        {
            await ConfigurePollIntervalAsync();
            await LoadFilesAsync();
            _filePollTimer.Start();
            return;
        }

        _filePollTimer.Stop();
    }

    public void SetSelectedFileNodes(IReadOnlyList<TorrentFileNodeItemViewModel> nodes)
    {
        _selectedFileNodes = nodes;
        SelectedFileNode = nodes.Count switch
        {
            0 => null,
            1 => nodes[0],
            _ => nodes[^1],
        };
        NotifyFileSelectionChanged();
    }

    [RelayCommand]
    private async Task RenameSelectedFileAsync()
    {
        if (GetSelectedFileNodes() is not [var node] || node.IsFolder)
            return;

        var dialog = new RenameFileDialog(node.Path, node.Name);
        var owner = GetOwnerWindow();
        if (owner is not null)
            await dialog.ShowDialog(owner);

        if (!dialog.Confirmed || dialog.NewName == node.Name)
            return;

        try
        {
            var dto = new TorrentActionDto(
                "rename-path",
                [_torrentId],
                false,
                null,
                null,
                false,
                node.Path,
                dialog.NewName);
            await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<ExecuteTorrentActionHandler>().HandleAsync(dto));
            if (_filesTabActive)
                await LoadFilesAsync();
            else
                await LoadSummaryAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task SetFilePriorityAsync(string priority)
    {
        var fileIndices = GetSelectedFileIndices();
        if (fileIndices.Count == 0)
            return;

        try
        {
            var dto = new TorrentActionDto(
                "set-file-priority",
                [_torrentId],
                false,
                priority,
                FileIndices: fileIndices);
            await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<ExecuteTorrentActionHandler>().HandleAsync(dto));
            if (_filesTabActive)
                await RefreshFileProgressAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task MassRenameAllAsync() => await OpenMassRenameAsync(string.Empty);

    [RelayCommand]
    private async Task MassRenameSelectedFolderAsync()
    {
        if (GetSelectedFileNodes() is not [var node] || !node.IsFolder)
            return;
        await OpenMassRenameAsync(node.Path);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _filesTabActive = false;
        _filePollTimer.Stop();
        _filePollTimer.Tick -= _filePollTickHandler;
        _localization.LanguageChanged -= RefreshLabels;
        FileTree.Clear();
        SelectedFileNode = null;
        _selectedFileNodes = Array.Empty<TorrentFileNodeItemViewModel>();
    }

    private async Task OnFilePollTickAsync()
    {
        if (_disposed)
            return;

        await RefreshFileProgressAsync();
    }

    private async Task LoadFilesAsync()
    {
        if (_disposed)
            return;

        try
        {
            var details = await FetchDetailsAsync();
            if (details is null)
            {
                ErrorMessage = "Torrent not found";
                return;
            }

            var selectedPaths = GetSelectedFileNodes().Select(node => node.Path).ToList();
            var tree = TorrentFileTreeBuilder.Build(details.Files);

            if (!_filesLoaded || FileTree.Count == 0)
            {
                FileTree = TorrentFileNodeItemViewModel.FromDtos(tree, _localization);
                _filesLoaded = true;
                if (selectedPaths.Count > 0)
                    FileTreeSelectionRestoreRequested?.Invoke(selectedPaths);
            }
            else
            {
                ApplyFileTree(tree);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task RefreshFileProgressAsync()
    {
        if (_disposed || !_filesTabActive)
            return;

        try
        {
            var details = await FetchDetailsAsync();
            if (details is null)
                return;

            ApplyFileTree(TorrentFileTreeBuilder.Build(details.Files));
        }
        catch
        {
        }
    }

    private void ApplyFileTree(IReadOnlyList<TorrentFileNodeDto> tree)
    {
        if (FileTree.Count == 0)
        {
            FileTree = TorrentFileNodeItemViewModel.FromDtos(tree, _localization);
            return;
        }

        var byPath = tree.ToDictionary(node => node.Path, StringComparer.Ordinal);
        foreach (var node in FileTree)
        {
            if (byPath.TryGetValue(node.Path, out var fresh))
                node.Apply(fresh);
        }
    }

    private async Task ConfigurePollIntervalAsync()
    {
        var settings = await _handlers.InvokeAsync(sp =>
            sp.GetRequiredService<GetSettingsHandler>().HandleAsync());
        _filePollTimer.Interval = TimeSpan.FromSeconds(Math.Max(1, settings.Ui.RefreshIntervalSeconds));
    }

    private async Task<TorrentDetails?> FetchDetailsAsync() =>
        await _handlers.InvokeAsync(sp =>
            sp.GetRequiredService<GetTorrentDetailsHandler>().HandleAsync(_torrentId));

    private void ApplySummary(TorrentDetails details)
    {
        Name = details.Name;
        Status = DisplayFormatter.Status(details.Status);
        Progress = DisplayFormatter.Percent(details.PercentDone);
        DownloadDir = details.DownloadDir;
        Hash = details.HashString;
        Comment = details.Comment;
        TransferSummary =
            $"{DisplayFormatter.Speed(details.RateDownload)} ↓ · {DisplayFormatter.Speed(details.RateUpload)} ↑ · {DisplayFormatter.Bytes(details.DownloadedEver)} / {DisplayFormatter.Bytes(details.TotalSize)}";
    }

    private async Task OpenMassRenameAsync(string scopePath)
    {
        if (!_filesLoaded)
            await LoadFilesAsync();

        var window = new MassRenameWindow(_torrentId, scopePath, FileTree.Select(node => node.ToDto()).ToList());
        var owner = GetOwnerWindow();
        if (owner is not null)
            await window.ShowDialog(owner);
        else
            window.Show();

        if (window.Applied && _filesTabActive)
            await LoadFilesAsync();
    }

    private void RefreshLabels()
    {
        TabGeneral = _localization.T("torrentDetails.tabs.general");
        TabTransfer = _localization.T("torrentDetails.tabs.transfer");
        TabFiles = _localization.T("torrentDetails.tabs.files");
        MassRenameAllLabel = _localization.T("torrentDetails.fileTree.massRenameAll");
        RenameFileLabel = _localization.T("torrentDetails.fileTree.rename");
        MassRenameFolderLabel = _localization.T("torrentDetails.fileTree.massRenameFolder");
        FilePriorityLabel = _localization.T("torrentTable.contextMenu.priority");
        PriorityHighLabel = _localization.T("torrentTable.contextMenu.high");
        PriorityNormalLabel = _localization.T("torrentTable.contextMenu.normal");
        PriorityLowLabel = _localization.T("torrentTable.contextMenu.low");
        FileColumnName = _localization.T("torrentDetails.fileTree.columnName");
        FileColumnPriority = _localization.T("torrentDetails.fileTree.columnPriority");
        FileColumnProgress = _localization.T("torrentDetails.fileTree.columnProgress");
    }

    private IReadOnlyList<TorrentFileNodeItemViewModel> GetSelectedFileNodes() =>
        _selectedFileNodes.Count > 0
            ? _selectedFileNodes
            : SelectedFileNode is not null
                ? [SelectedFileNode]
                : Array.Empty<TorrentFileNodeItemViewModel>();

    private IReadOnlyList<int> GetSelectedFileIndices() =>
        GetSelectedFileNodes()
            .Where(node => !node.IsFolder && node.FileIndex.HasValue)
            .Select(node => node.FileIndex!.Value)
            .Distinct()
            .ToList();

    private void NotifyFileSelectionChanged()
    {
        OnPropertyChanged(nameof(CanRenameFile));
        OnPropertyChanged(nameof(CanMassRenameFolder));
        OnPropertyChanged(nameof(CanSetFilePriority));
    }

    private static global::Avalonia.Controls.Window? GetOwnerWindow() =>
        global::Avalonia.Application.Current?.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
}
