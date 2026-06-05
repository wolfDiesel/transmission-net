using Avalonia.Controls;
using Avalonia.Interactivity;
using TransmissonNET.App.Avalonia.Desktop;

namespace TransmissonNET.App.Avalonia.Views;

public partial class RenameFileDialog : Window
{
    public bool Confirmed { get; private set; }
    public string NewName => NameBox.Text?.Trim() ?? string.Empty;

    public RenameFileDialog(string path, string currentName)
    {
        InitializeComponent();
        PathText.Text = path;
        NameBox.Text = currentName;
        WindowEscClose.Attach(this);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close();

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewName)
            || NewName.Contains('/', StringComparison.Ordinal)
            || NewName.Contains('\\', StringComparison.Ordinal))
            return;

        Confirmed = true;
        Close();
    }
}
