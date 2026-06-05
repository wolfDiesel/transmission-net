using CommunityToolkit.Mvvm.ComponentModel;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class TorrentColumnItemViewModel : ViewModelBase
{
    public string Id { get; init; } = string.Empty;

    internal Action<string, bool>? VisibilityChanged { get; set; }

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private bool _visible;

    partial void OnVisibleChanged(bool value) => VisibilityChanged?.Invoke(Id, value);
}
