using Avalonia.Interactivity;
using TransmissonNET.App.Avalonia.Desktop;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.App.Avalonia.ViewModels;
using TransmissonNET.Application.Contracts;

namespace TransmissonNET.App.Avalonia.Views;

public partial class MassRenameWindow : global::Avalonia.Controls.Window
{
    private readonly MassRenameViewModel _viewModel;

    public bool Applied => _viewModel.Applied;

    public MassRenameWindow(int torrentId, string scopePath, IReadOnlyList<TorrentFileNodeDto> fileTree)
    {
        InitializeComponent();
        _viewModel = new MassRenameViewModel(
            torrentId,
            scopePath,
            fileTree,
            AppServices.GetRequired<HandlerInvoker>(),
            AppServices.GetRequired<LocalizationService>());
        DataContext = _viewModel;
        WindowEscClose.Attach(this);
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MassRenameViewModel.Applied) && _viewModel.Applied)
                Close();
        };
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close();
}
