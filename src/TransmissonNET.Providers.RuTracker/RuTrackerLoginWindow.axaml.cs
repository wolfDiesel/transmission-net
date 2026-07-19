using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TransmissonNET.Providers.RuTracker;

public partial class RuTrackerLoginWindow : Window
{
    public RuTrackerLoginWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public string Username => this.FindControl<TextBox>("UsernameBox")?.Text?.Trim() ?? string.Empty;

    public string Password => this.FindControl<TextBox>("PasswordBox")?.Text ?? string.Empty;

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        Close(false);

    private void OnLoginClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            var error = this.FindControl<TextBlock>("ErrorText");
            if (error is not null)
            {
                error.Text = "Enter username and password.";
                error.IsVisible = true;
            }

            return;
        }

        Close(true);
    }
}
