using CommunityToolkit.Mvvm.ComponentModel;
using TransmissonNET.Application.Contracts;
using TransmissonNET.App.Avalonia.Services;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class TorrentRowViewModel : ViewModelBase
{
    public int Id { get; private init; }

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _status = string.Empty;
    [ObservableProperty] private double _percentDone;
    [ObservableProperty] private string _progress = string.Empty;
    [ObservableProperty] private string _size = string.Empty;
    [ObservableProperty] private string _downloadSpeed = string.Empty;
    [ObservableProperty] private string _uploadSpeed = string.Empty;
    [ObservableProperty] private string _eta = string.Empty;
    [ObservableProperty] private string _uploadRatio = string.Empty;
    [ObservableProperty] private string _peers = string.Empty;
    [ObservableProperty] private string _downloaded = string.Empty;
    [ObservableProperty] private string _uploaded = string.Empty;
    [ObservableProperty] private string _queue = string.Empty;
    [ObservableProperty] private string _downloadDir = string.Empty;
    [ObservableProperty] private string _left = string.Empty;
    [ObservableProperty] private string _addedDate = string.Empty;
    [ObservableProperty] private string _doneDate = string.Empty;

    public TorrentDto Source { get; private set; } = null!;

    public static TorrentRowViewModel FromDto(TorrentDto dto)
    {
        var row = new TorrentRowViewModel { Id = dto.Id };
        row.Apply(dto);
        return row;
    }

    public void UpdateFrom(TorrentDto dto)
    {
        if (dto.Id != Id)
            throw new InvalidOperationException("Cannot change torrent row id.");
        Apply(dto);
    }

    private void Apply(TorrentDto dto)
    {
        Source = dto;
        Name = dto.Name;
        Status = DisplayFormatter.Status(dto.Status);
        PercentDone = dto.PercentDone;
        Progress = DisplayFormatter.Percent(dto.PercentDone);
        Size = DisplayFormatter.Bytes(dto.TotalSize);
        DownloadSpeed = DisplayFormatter.Speed(dto.RateDownload);
        UploadSpeed = DisplayFormatter.Speed(dto.RateUpload);
        Eta = DisplayFormatter.Eta(dto.Eta);
        UploadRatio = dto.UploadRatio.ToString("0.##");
        Peers = dto.PeersConnected.ToString();
        Downloaded = DisplayFormatter.Bytes(dto.DownloadedEver);
        Uploaded = DisplayFormatter.Bytes(dto.UploadedEver);
        Queue = dto.QueuePosition.ToString();
        DownloadDir = dto.DownloadDir;
        Left = DisplayFormatter.Bytes(dto.LeftUntilDone);
        AddedDate = DisplayFormatter.UnixDate(dto.AddedDate);
        DoneDate = DisplayFormatter.UnixDate(dto.DoneDate);
    }
}
