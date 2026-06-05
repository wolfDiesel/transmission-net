using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.App.Avalonia.Services;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class AddTorrentViewModel : ViewModelBase
{
    private readonly HandlerInvoker _handlers;
    private readonly LocalizationService _localization;
    private readonly AppToastService _toasts;
    private string? _metainfoBase64;

    [ObservableProperty] private string _torrentFilePath = string.Empty;
    [ObservableProperty] private string _downloadDir = string.Empty;
    [ObservableProperty] private bool _addPaused;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _previewTitle = string.Empty;
    [ObservableProperty] private string _previewMeta = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _previewFiles = new();
    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _pageSubtitle = string.Empty;
    [ObservableProperty] private string _torrentFileLabel = string.Empty;
    [ObservableProperty] private string _browseLabel = string.Empty;
    [ObservableProperty] private string _downloadDirLabel = string.Empty;
    [ObservableProperty] private string _addPausedLabel = string.Empty;
    [ObservableProperty] private string _addLabel = string.Empty;

    public bool HasPreview => !string.IsNullOrWhiteSpace(PreviewTitle);

    public AddTorrentViewModel(HandlerInvoker handlers, LocalizationService localization, AppToastService toasts)
    {
        _handlers = handlers;
        _localization = localization;
        _toasts = toasts;
        _localization.LanguageChanged += RefreshLabels;
        RefreshLabels();
    }

    public async Task InitializeAsync()
    {
        var settings = await _handlers.InvokeAsync(sp => sp.GetRequiredService<GetSettingsHandler>().HandleAsync());
        DownloadDir = settings.Ui.DownloadDirHistory?.FirstOrDefault() ?? string.Empty;
    }

    public async Task OpenFromPathAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            _toasts.ShowError(_localization.T("addTorrent.readFailed"), path);
            return;
        }

        TorrentFilePath = path;
        await InspectFileAsync(path);
    }

    [RelayCommand]
    private async Task BrowseTorrentAsync()
    {
        var owner = GetOwnerWindow();
        if (owner?.StorageProvider is not { } storage)
            return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = _localization.T("addTorrent.torrentFile"),
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Torrent") { Patterns = ["*.torrent"] },
            ],
        });

        var file = files.FirstOrDefault();
        if (file is null)
            return;

        TorrentFilePath = file.Path.LocalPath;
        await InspectFileAsync(TorrentFilePath);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(_metainfoBase64))
        {
            _toasts.ShowError(_localization.T("addTorrent.selectTorrent"));
            return;
        }

        if (string.IsNullOrWhiteSpace(DownloadDir))
        {
            _toasts.ShowError(_localization.T("addTorrent.dirRequired"));
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<AddTorrentHandler>().HandleAsync(
                    new TorrentAddRequestDto(_metainfoBase64, DownloadDir.Trim(), AddPaused)));
            _toasts.ShowSuccess(_localization.Format("addTorrent.added", ("name", result.Name)));
        }
        catch (Exception ex)
        {
            _toasts.ShowError(_localization.T("addTorrent.addFailed"), ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task InspectFileAsync(string path)
    {
        IsBusy = true;
        try
        {
            var inspected = await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<InspectTorrentMetainfoFromPathHandler>()
                    .HandleAsync(new TorrentMetainfoInspectPathRequestDto(path)));
            _metainfoBase64 = inspected.MetainfoBase64;
            PreviewTitle = inspected.Preview.Name;
            PreviewMeta = _localization.Format(
                "addTorrent.fileMeta",
                ("file", path),
                ("size", DisplayFormatter.Bytes(inspected.Preview.TotalSize)));
            PreviewFiles = new ObservableCollection<string>(FlattenTree(inspected.Preview.FileTree));
            OnPropertyChanged(nameof(HasPreview));
        }
        catch (Exception ex)
        {
            _metainfoBase64 = null;
            PreviewTitle = string.Empty;
            PreviewMeta = string.Empty;
            PreviewFiles.Clear();
            OnPropertyChanged(nameof(HasPreview));
            _toasts.ShowError(_localization.T("addTorrent.readFailed"), ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnPreviewTitleChanged(string value) => OnPropertyChanged(nameof(HasPreview));

    private void RefreshLabels()
    {
        PageTitle = _localization.T("addTorrent.title");
        PageSubtitle = _localization.T("addTorrent.subtitle");
        TorrentFileLabel = _localization.T("addTorrent.torrentFile");
        BrowseLabel = _localization.T("addTorrent.browse");
        DownloadDirLabel = _localization.T("addTorrent.downloadDir");
        AddPausedLabel = _localization.T("addTorrent.addPaused");
        AddLabel = _localization.T("addTorrent.add");
    }

    private static IEnumerable<string> FlattenTree(IReadOnlyList<TorrentFileNodeDto> nodes, int depth = 0)
    {
        foreach (var node in nodes)
        {
            if (!node.IsFolder)
            {
                yield return $"{new string(' ', depth * 2)}{node.Path} ({DisplayFormatter.Bytes(node.Length)})";
                continue;
            }

            foreach (var child in FlattenTree(node.Children, depth + 1))
                yield return child;
        }
    }

    private static global::Avalonia.Controls.Window? GetOwnerWindow() =>
        global::Avalonia.Application.Current?.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
}
