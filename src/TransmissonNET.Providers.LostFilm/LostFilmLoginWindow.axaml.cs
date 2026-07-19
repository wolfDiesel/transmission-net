using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TransmissonNET.Providers.LostFilm;

public partial class LostFilmLoginWindow : Window
{
    public LostFilmLoginWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public string Email => this.FindControl<TextBox>("EmailBox")?.Text?.Trim() ?? string.Empty;

    public string Password => this.FindControl<TextBox>("PasswordBox")?.Text ?? string.Empty;

    public string SessionCookie => this.FindControl<TextBox>("CookieBox")?.Text?.Trim() ?? string.Empty;

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        Close(false);

    private void OnLoginClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SessionCookie)
            && (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password)))
        {
            var error = this.FindControl<TextBlock>("ErrorText");
            if (error is not null)
            {
                error.Text = "Enter email/password or lf_session cookie.";
                error.IsVisible = true;
            }

            return;
        }

        Close(true);
    }
}
