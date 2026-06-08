using Avalonia.Controls;
using Avalonia.Interactivity;
using TransmissonNET.App.Avalonia.Desktop;
using TransmissonNET.App.Avalonia.Services;

namespace TransmissonNET.App.Avalonia.Views;

public partial class MoveTorrentDialog : Window
{
    public bool Confirmed { get; private set; }
    public string Destination => DestinationBox.Text ?? string.Empty;
    public bool MoveData => MoveDataCheck.IsChecked == true;

    public MoveTorrentDialog(string currentDir, IReadOnlyList<string> history)
    {
        InitializeComponent();
        var localization = AppServices.GetRequired<LocalizationService>();
        DestinationBox.ItemsSource = history;
        DestinationBox.Text = currentDir;
        DestinationBox.PlaceholderText = localization.T("torrentTable.contextMenu.move");
        WindowEscClose.Attach(this);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close();

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        Confirmed = true;
        Close();
    }
}
