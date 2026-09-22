using TransmissonNET.Application.Abstractions;
using TransmissonNET.App.Avalonia.ViewModels;

namespace TransmissonNET.App.Avalonia.Services;

internal sealed class AddTorrentCompletionActions(
    DownloadDirHistoryService downloadDirHistory,
    NavigationService navigation,
    TorrentsViewModel torrents) : IAddTorrentCompletionActions
{
    public void RememberDownloadDirectory(string path) => downloadDirHistory.Remember(path);

    public void SwitchToTorrentList() => navigation.Navigate(AppPage.Torrents);

    public Task FocusTorrentAsync(int id) => torrents.SelectTorrentAsync(id);
}
