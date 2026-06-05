using Avalonia.Controls;
using Avalonia.Interactivity;
using TransmissonNET.App.Avalonia.Desktop;

namespace TransmissonNET.App.Avalonia.Views;

public partial class RemoveTorrentDialog : Window
{
    public bool Confirmed { get; private set; }
    public bool DeleteLocalData => DeleteDataCheck.IsChecked == true;

    public RemoveTorrentDialog(string torrentName)
    {
        InitializeComponent();
        TorrentNameText.Text = $"You are about to remove: {torrentName}";
        WindowEscClose.Attach(this);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close();

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        Confirmed = true;
        Close();
    }
}
