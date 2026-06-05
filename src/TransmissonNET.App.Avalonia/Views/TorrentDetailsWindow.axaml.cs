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
    }

    private async void OnLoaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is TorrentDetailsViewModel vm)
            await vm.LoadSummaryAsync();
    }

    private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not TorrentDetailsViewModel vm)
            return;

        vm.SetFilesTabActive(DetailsTabs.SelectedIndex == FilesTabIndex);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is TorrentDetailsViewModel vm)
            vm.Dispose();
    }
}
