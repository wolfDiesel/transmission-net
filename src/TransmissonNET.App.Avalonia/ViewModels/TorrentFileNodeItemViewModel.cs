using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using TransmissonNET.Application.Contracts;
using TransmissonNET.App.Avalonia.Services;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class TorrentFileNodeItemViewModel : ViewModelBase
{
    private readonly LocalizationService? _localization;

    [ObservableProperty] private long _bytesCompleted;
    [ObservableProperty] private long _length;
    [ObservableProperty] private int? _priority;

    public TorrentFileNodeItemViewModel(TorrentFileNodeDto dto, LocalizationService? localization = null)
    {
        _localization = localization;
        Name = dto.Name;
        Path = dto.Path;
        IsFolder = dto.IsFolder;
        FileIndex = dto.FileIndex;
        BytesCompleted = dto.BytesCompleted;
        Length = dto.Length;
        Priority = dto.Priority;
        Children = new ObservableCollection<TorrentFileNodeItemViewModel>(
            dto.Children.Select(child => new TorrentFileNodeItemViewModel(child, localization)));
    }

    public string Name { get; }
    public string Path { get; }
    public bool IsFolder { get; }
    public int? FileIndex { get; }

    public ObservableCollection<TorrentFileNodeItemViewModel> Children { get; }

    public double PercentDone =>
        Length > 0 ? Math.Clamp((double)BytesCompleted / Length, 0, 1) : 0;

    public bool ShowPriority => !IsFolder;

    public string PriorityGlyph => !IsFolder
        ? Priority switch
        {
            < 0 => "▼",
            > 0 => "▲",
            _ => "●",
        }
        : string.Empty;

    public string PriorityToolTip =>
        !IsFolder && _localization is not null
            ? Priority switch
            {
                < 0 => _localization.T("torrentTable.contextMenu.low"),
                > 0 => _localization.T("torrentTable.contextMenu.high"),
                _ => _localization.T("torrentTable.contextMenu.normal"),
            }
            : string.Empty;

    public IBrush? PriorityBrush
    {
        get
        {
            if (IsFolder)
                return null;

            var key = Priority > 0 ? "ProgressFillBrush" : "ForegroundBrush";
            var app = global::Avalonia.Application.Current;
            return app?.Resources.TryGetValue(key, out var resource) == true && resource is IBrush brush
                ? brush
                : Brushes.Gray;
        }
    }

    partial void OnBytesCompletedChanged(long value)
    {
        OnPropertyChanged(nameof(PercentDone));
    }

    partial void OnLengthChanged(long value)
    {
        OnPropertyChanged(nameof(PercentDone));
    }

    partial void OnPriorityChanged(int? value)
    {
        OnPropertyChanged(nameof(PriorityGlyph));
        OnPropertyChanged(nameof(ShowPriority));
        OnPropertyChanged(nameof(PriorityBrush));
        OnPropertyChanged(nameof(PriorityToolTip));
    }

    public static ObservableCollection<TorrentFileNodeItemViewModel> FromDtos(
        IReadOnlyList<TorrentFileNodeDto> nodes,
        LocalizationService? localization = null) =>
        new(nodes.Select(node => new TorrentFileNodeItemViewModel(node, localization)));

    public TorrentFileNodeDto ToDto() =>
        new(
            Name,
            Path,
            IsFolder,
            FileIndex,
            Length,
            BytesCompleted,
            null,
            Priority,
            Children.Select(child => child.ToDto()).ToList());

    public void Apply(TorrentFileNodeDto dto)
    {
        BytesCompleted = dto.BytesCompleted;
        Length = dto.Length;
        Priority = dto.Priority;

        if (!IsFolder)
            return;

        var byPath = dto.Children.ToDictionary(child => child.Path, StringComparer.Ordinal);
        foreach (var child in Children)
        {
            if (byPath.TryGetValue(child.Path, out var fresh))
                child.Apply(fresh);
        }
    }

    public static TorrentFileNodeItemViewModel? FindByPath(
        IEnumerable<TorrentFileNodeItemViewModel> nodes,
        string path)
    {
        foreach (var node in nodes)
        {
            if (node.Path == path)
                return node;

            if (node.IsFolder)
            {
                var found = FindByPath(node.Children, path);
                if (found is not null)
                    return found;
            }
        }

        return null;
    }
}
