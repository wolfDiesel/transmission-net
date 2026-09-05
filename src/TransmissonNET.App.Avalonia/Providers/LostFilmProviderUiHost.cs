using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.App.Avalonia.Providers;

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

    private void OnCancelClick(object? sender, RoutedEventArgs e) =>
        Close(false);

    private void OnLoginClick(object? sender, RoutedEventArgs e)
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

/// <summary>
/// Host implementation of <see cref="IProviderUiHost"/> for LostFilm: shows the
/// email/password/cookie login window. The plugin only consumes the host contract.
/// </summary>
public sealed class LostFilmProviderUiHost : IProviderUiHost
{
    public async Task<ProviderLoginResult?> LoginAsync(
        string providerId,
        string baseUrl,
        string dataDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(providerId, "lostfilm", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"LostFilmProviderUiHost cannot handle provider '{providerId}'.");

        var owner = GetOwnerWindow()
            ?? throw new InvalidOperationException("No application window available for LostFilm login.");

        var window = new LostFilmLoginWindow();
        var accepted = await window.ShowDialog<bool?>(owner);
        if (accepted != true)
            return null;

        return new ProviderLoginResult(
            Cookies: [],
            UserAgent: null,
            SessionCookie: window.SessionCookie.Trim(),
            Email: window.Email,
            Password: window.Password);
    }

    private static Window? GetOwnerWindow()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}
