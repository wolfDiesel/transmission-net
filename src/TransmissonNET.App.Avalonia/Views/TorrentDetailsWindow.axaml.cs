using Avalonia.Controls;
using TransmissonNET.App.Avalonia.Desktop;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.App.Avalonia.ViewModels;

namespace TransmissonNET.App.Avalonia.Views;

public partial class TorrentDetailsWindow : Window
{
    private const int FilesTabIndex = 2;

    public TorrentDetailsWindow(int torrentId, string title)
    {
        InitializeComponent();
        WindowEscClose.Attach(this);
        DataContext = new TorrentDetailsViewModel(
            AppServices.GetRequired<HandlerInvoker>(),
            AppServices.GetRequired<LocalizationService>(),
            torrentId,
            title);

        Loaded += OnLoaded;
        Closed += OnClosed;
        DetailsTabs.SelectionChanged += OnTabSelectionChanged;
        FileTreeView.SelectionChanged += OnFileTreeSelectionChanged;
    }

    private async void OnLoaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not TorrentDetailsViewModel vm)
            return;

        vm.FileTreeSelectionRestoreRequested += RestoreFileTreeSelection;
        await vm.LoadSummaryAsync();
    }

    private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not TorrentDetailsViewModel vm)
            return;

        vm.SetFilesTabActive(DetailsTabs.SelectedIndex == FilesTabIndex);
    }

    private void OnFileTreeSelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        SyncFileTreeSelection();

    private void SyncFileTreeSelection()
    {
        if (DataContext is not TorrentDetailsViewModel vm)
            return;

        var selected = FileTreeView.SelectedItems
            .OfType<TorrentFileNodeItemViewModel>()
            .ToList();
        vm.SetSelectedFileNodes(selected);
    }

    private void RestoreFileTreeSelection(IReadOnlyList<string> paths)
    {
        if (paths.Count == 0)
            return;

        var pathSet = paths.ToHashSet(StringComparer.Ordinal);
        var nodes = new List<TorrentFileNodeItemViewModel>();
        CollectNodesByPaths(FileTreeView.ItemsSource, pathSet, nodes);

        FileTreeView.SelectedItems.Clear();
        foreach (var node in nodes)
            FileTreeView.SelectedItems.Add(node);

        SyncFileTreeSelection();
    }

    private static void CollectNodesByPaths(
        object? items,
        HashSet<string> paths,
        List<TorrentFileNodeItemViewModel> result)
    {
        if (items is not System.Collections.IEnumerable enumerable)
            return;

        foreach (var item in enumerable)
        {
            if (item is not TorrentFileNodeItemViewModel node)
                continue;

            if (paths.Contains(node.Path))
                result.Add(node);

            if (node.IsFolder)
                CollectNodesByPaths(node.Children, paths, result);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;
        Closed -= OnClosed;
        DetailsTabs.SelectionChanged -= OnTabSelectionChanged;
        FileTreeView.SelectionChanged -= OnFileTreeSelectionChanged;

        if (DataContext is TorrentDetailsViewModel vm)
        {
            vm.FileTreeSelectionRestoreRequested -= RestoreFileTreeSelection;
            vm.Dispose();
            DataContext = null;
        }
    }
}
