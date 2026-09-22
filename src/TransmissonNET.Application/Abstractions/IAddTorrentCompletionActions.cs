namespace TransmissonNET.Application.Abstractions;

public interface IAddTorrentCompletionActions
{
    void RememberDownloadDirectory(string path);

    void SwitchToTorrentList();

    Task FocusTorrentAsync(int id);
}
