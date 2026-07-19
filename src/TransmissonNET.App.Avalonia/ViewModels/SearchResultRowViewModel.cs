using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransmissonNET.App.Avalonia.Services;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class SearchResultRowViewModel : ViewModelBase
{
    private readonly Func<SearchResultRowViewModel, Task> _download;
    private readonly Func<SearchResultRowViewModel, Task> _openLink;

    public SearchResultRowViewModel(
        string providerId,
        string providerDisplayName,
        string hitId,
        string title,
        long? sizeBytes,
        string? detailUrl,
        string downloadButtonText,
        string openLinkButtonText,
        Func<SearchResultRowViewModel, Task> download,
        Func<SearchResultRowViewModel, Task> openLink)
    {
        ProviderId = providerId;
        ProviderDisplayName = providerDisplayName;
        HitId = hitId;
        Title = title;
        SizeBytes = sizeBytes;
        DetailUrl = detailUrl;
        SizeText = sizeBytes is { } bytes ? DisplayFormatter.Bytes(bytes) : "—";
        HasLink = !string.IsNullOrWhiteSpace(detailUrl);
        DownloadButtonText = downloadButtonText;
        OpenLinkButtonText = openLinkButtonText;
        _download = download;
        _openLink = openLink;
    }

    public string ProviderId { get; }
    public string ProviderDisplayName { get; }
    public string HitId { get; }
    public string Title { get; }
    public long? SizeBytes { get; }
    public string? DetailUrl { get; }
    public string SizeText { get; }
    public bool HasLink { get; }
    public string DownloadButtonText { get; }
    public string OpenLinkButtonText { get; }

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private Task DownloadAsync() => _download(this);

    [RelayCommand]
    private Task OpenLinkAsync() => _openLink(this);
}
