using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Application.Settings;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.App.Avalonia.Views;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class TorrentDetailsViewModel : ViewModelBase
{
    private readonly HandlerInvoker _handlers;
    private readonly int _torrentId;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _status = string.Empty;
    [ObservableProperty] private string _progress = string.Empty;
    [ObservableProperty] private string _downloadDir = string.Empty;
    [ObservableProperty] private string _hash = string.Empty;
    [ObservableProperty] private string _comment = string.Empty;
    [ObservableProperty] private string _transferSummary = string.Empty;
    [ObservableProperty] private ObservableCollection<TorrentFileNodeDto> _fileTree = new();
    [ObservableProperty] private TorrentFileNodeDto? _selectedFileNode;
    [ObservableProperty] private string? _errorMessage;

    public TorrentDetailsViewModel(HandlerInvoker handlers, int torrentId, string title)
    {
        _handlers = handlers;
        _torrentId = torrentId;
        Name = title;
    }

    public async Task LoadAsync()
    {
        try
        {
            var details = await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<GetTorrentDetailsHandler>().HandleAsync(_torrentId));
            if (details is null)
            {
                ErrorMessage = "Torrent not found";
                return;
            }

            Name = details.Name;
            Status = DisplayFormatter.Status(details.Status);
            Progress = DisplayFormatter.Percent(details.PercentDone);
            DownloadDir = details.DownloadDir;
            Hash = details.HashString;
            Comment = details.Comment;
            TransferSummary =
                $"{DisplayFormatter.Speed(details.RateDownload)} ↓ · {DisplayFormatter.Speed(details.RateUpload)} ↑ · {DisplayFormatter.Bytes(details.DownloadedEver)} / {DisplayFormatter.Bytes(details.TotalSize)}";

            var selectedPath = SelectedFileNode?.Path;
            var tree = TorrentFileTreeBuilder.Build(details.Files);
            FileTree = new ObservableCollection<TorrentFileNodeDto>(tree);
            if (!string.IsNullOrEmpty(selectedPath))
                SelectedFileNode = FindNodeByPath(FileTree, selectedPath);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task RenameSelectedFileAsync()
    {
        if (SelectedFileNode is null)
            return;

        var dialog = new RenameFileDialog(SelectedFileNode.Path, SelectedFileNode.Name);
        var owner = GetOwnerWindow();
        if (owner is not null)
            await dialog.ShowDialog(owner);

        if (!dialog.Confirmed || dialog.NewName == SelectedFileNode.Name)
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
                SelectedFileNode.Path,
                dialog.NewName);
            await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<ExecuteTorrentActionHandler>().HandleAsync(dto));
            await LoadAsync();
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
        if (SelectedFileNode is not { IsFolder: true })
            return;
        await OpenMassRenameAsync(SelectedFileNode.Path);
    }

    private async Task OpenMassRenameAsync(string scopePath)
    {
        var window = new MassRenameWindow(_torrentId, scopePath, FileTree.ToList());
        var owner = GetOwnerWindow();
        if (owner is not null)
            await window.ShowDialog(owner);
        else
            window.Show();

        if (window.Applied)
            await LoadAsync();
    }

    private static TorrentFileNodeDto? FindNodeByPath(
        IEnumerable<TorrentFileNodeDto> nodes,
        string path)
    {
        foreach (var node in nodes)
        {
            if (node.Path == path)
                return node;
            if (node.IsFolder)
            {
                var found = FindNodeByPath(node.Children, path);
                if (found is not null)
                    return found;
            }
        }

        return null;
    }

    private static global::Avalonia.Controls.Window? GetOwnerWindow() =>
        global::Avalonia.Application.Current?.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
}
