using Avalonia.Controls;
using Avalonia.Interactivity;
using TransmissonNET.App.Avalonia.Desktop;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.App.Avalonia.ViewModels;

namespace TransmissonNET.App.Avalonia.Views;

internal partial class AboutWindow : Window
{
    public AboutWindow(LocalizationService localization)
    {
        InitializeComponent();
        WindowEscClose.Attach(this);

        Title = localization.T("about.title");
        VersionLabel.Text = localization.T("about.version");
        CloseButton.Content = localization.T("about.close");
        VersionValue.Text = AppVersionInfo.Version;
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();

    public static void Show(Window? owner, LocalizationService localization)
    {
        var window = new AboutWindow(localization);
        if (owner is not null)
            window.Show(owner);
        else
            window.Show();
    }
}