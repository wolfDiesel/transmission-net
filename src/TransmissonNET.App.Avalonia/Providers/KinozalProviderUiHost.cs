using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.App.Avalonia.Providers;

public partial class KinozalLoginWindow : Window
{
    public KinozalLoginWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public string Username => this.FindControl<TextBox>("UsernameBox")?.Text?.Trim() ?? string.Empty;

    public string Password => this.FindControl<TextBox>("PasswordBox")?.Text ?? string.Empty;

    private void OnCancelClick(object? sender, RoutedEventArgs e) =>
        Close(false);

    private void OnLoginClick(object? sender, RoutedEventArgs e)
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

/// <summary>
/// Host implementation of <see cref="IProviderUiHost"/> for Kinozal: shows the
/// username/password login window. The plugin only consumes the host contract.
/// </summary>
public sealed class KinozalProviderUiHost : IProviderUiHost
{
    public async Task<ProviderLoginResult?> LoginAsync(
        string providerId,
        string baseUrl,
        string dataDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(providerId, "kinozal", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"KinozalProviderUiHost cannot handle provider '{providerId}'.");

        var owner = GetOwnerWindow()
            ?? throw new InvalidOperationException("No application window available for Kinozal login.");

        var window = new KinozalLoginWindow();
        var accepted = await window.ShowDialog<bool?>(owner);
        if (accepted != true)
            return null;

        return new ProviderLoginResult(
            Cookies: [],
            UserAgent: null,
            SessionCookie: null,
            Email: window.Username,
            Password: window.Password);
    }

    private static Window? GetOwnerWindow()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}
