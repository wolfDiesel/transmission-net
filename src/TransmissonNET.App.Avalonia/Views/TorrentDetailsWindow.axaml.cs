using TransmissonNET.App.Avalonia.Desktop;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.App.Avalonia.ViewModels;

namespace TransmissonNET.App.Avalonia.Views;

public partial class TorrentDetailsWindow : global::Avalonia.Controls.Window
{
    public TorrentDetailsWindow(int torrentId, string title)
    {
        InitializeComponent();
        WindowEscClose.Attach(this);
        DataContext = new TorrentDetailsViewModel(AppServices.GetRequired<HandlerInvoker>(), torrentId, title);
        Loaded += async (_, _) =>
        {
            if (DataContext is TorrentDetailsViewModel vm)
                await vm.LoadAsync();
        };
    }
}
