using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class ProviderSettingsItemViewModel : ViewModelBase
{
    private readonly ITorrentProvider _provider;

    public ProviderSettingsItemViewModel(ITorrentProvider provider)
    {
        _provider = provider;
        DisplayName = provider.DisplayName;
        Id = provider.Id;
        ShowLostFilmExtras = string.Equals(provider.Id, "lostfilm", StringComparison.OrdinalIgnoreCase);
        foreach (var mirror in provider.KnownMirrors)
            MirrorOptions.Add(new ProviderMirrorOptionViewModel(mirror, SelectMirror));
        ReloadFromProvider();
    }

    public string Id { get; }

    public string DisplayName { get; }

    public bool ShowLostFilmExtras { get; }

    public bool HasKnownMirrors => MirrorOptions.Count > 0;

    public ObservableCollection<ProviderMirrorOptionViewModel> MirrorOptions { get; } = new();

    [ObservableProperty]
    private int _requestTimeoutSeconds = 10;

    [ObservableProperty]
    private string _baseUrl = string.Empty;

    [ObservableProperty]
    private string _preferredQuality = "1080";

    [ObservableProperty]
    private int _maxSeriesExpand = 3;

    partial void OnBaseUrlChanged(string value) => RefreshMirrorSelection();

    public void ReloadFromProvider()
    {
        var settings = _provider.GetSettings();
        RequestTimeoutSeconds = Math.Clamp(settings.RequestTimeoutSeconds, 1, 600);
        BaseUrl = settings.BaseUrl ?? string.Empty;
        PreferredQuality = string.IsNullOrWhiteSpace(settings.PreferredQuality)
            ? "1080"
            : settings.PreferredQuality;
        MaxSeriesExpand = settings.MaxSeriesExpand <= 0
            ? 3
            : Math.Clamp(settings.MaxSeriesExpand, 1, 20);
        RefreshMirrorSelection();
    }

    public void ApplyToProvider()
    {
        _provider.SetSettings(new TorrentProviderSettings
        {
            RequestTimeoutSeconds = Math.Clamp(RequestTimeoutSeconds, 1, 600),
            BaseUrl = string.IsNullOrWhiteSpace(BaseUrl) ? null : BaseUrl.Trim(),
            PreferredQuality = string.IsNullOrWhiteSpace(PreferredQuality) ? null : PreferredQuality.Trim(),
            MaxSeriesExpand = Math.Clamp(MaxSeriesExpand, 1, 20),
        });
    }

    private void SelectMirror(string url) => BaseUrl = url;

    private void RefreshMirrorSelection()
    {
        var current = NormalizeMirror(BaseUrl);
        foreach (var option in MirrorOptions)
            option.IsSelected = string.Equals(NormalizeMirror(option.Url), current, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeMirror(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;
        var value = url.Trim();
        if (!value.EndsWith('/'))
            value += "/";
        return value;
    }
}

internal sealed partial class ProviderMirrorOptionViewModel : ViewModelBase
{
    private readonly Action<string> _select;

    public ProviderMirrorOptionViewModel(string url, Action<string> select)
    {
        Url = url;
        _select = select;
    }

    public string Url { get; }

    [ObservableProperty]
    private bool _isSelected;

    [RelayCommand]
    private void Select() => _select(Url);
}
