using Avalonia.Controls;
using Avalonia.Interactivity;
using TransmissonNET.App.Avalonia.Desktop;

namespace TransmissonNET.App.Avalonia.Views;

public partial class MoveTorrentDialog : Window
{
    public bool Confirmed { get; private set; }
    public string Destination => DestinationBox.Text ?? string.Empty;
    public bool MoveData => MoveDataCheck.IsChecked == true;

    public MoveTorrentDialog(string currentDir)
    {
        InitializeComponent();
        DestinationBox.Text = currentDir;
        WindowEscClose.Attach(this);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close();

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        Confirmed = true;
        Close();
    }
}
