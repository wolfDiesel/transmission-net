using System.Net;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.App.Avalonia.Providers;

/// <summary>
/// Host implementation of <see cref="IProviderUiHost"/> for RuTracker.
/// The RuTracker plugin itself no longer references Avalonia; the host owns the login window.
/// </summary>
public sealed class RuTrackerProviderUiHost : IProviderUiHost
{
    public async Task<ProviderLoginResult?> LoginAsync(
        string providerId,
        string baseUrl,
        string dataDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(providerId, "rutracker", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"RuTrackerProviderUiHost cannot handle provider '{providerId}'.");

        var owner = GetOwnerWindow();
        var result = await RuTrackerWebLogin.ShowAsync(baseUrl, dataDirectory, owner, cancellationToken);
        if (result is null)
            return null;

        return new ProviderLoginResult(result.Value.Cookies, result.Value.UserAgent);
    }

    private static Window? GetOwnerWindow()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}

/// <summary>
/// Static logic moved from the RuTracker provider: shows a small control window plus a
/// native WebKit dialog, polls cookies from WebKit until a session is detected, and
/// returns the accepted session. The plugin only consumes <see cref="IProviderUiHost"/>.
/// </summary>
internal static class RuTrackerWebLogin
{
    public static async Task<(IReadOnlyList<Cookie> Cookies, string? UserAgent)?> ShowAsync(
        string baseUrl,
        string dataDirectory,
        Window? owner,
        CancellationToken cancellationToken = default)
    {
        ApplyWebKitGtkWorkarounds();

        var loginUrl = RuTrackerMirrorsNormalize(baseUrl).TrimEnd('/') + "/forum/login.php";
        var webData = Path.Combine(dataDirectory, "webview");
        Directory.CreateDirectory(webData);

        var status = new TextBlock
        {
            Text = "Sign in in the browser window, then click “Use session”.",
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.8,
        };
        var useButton = new Button { Content = "Use session", MinWidth = 120 };
        var cancelButton = new Button { Content = "Cancel", MinWidth = 100 };

        var panel = new StackPanel
        {
            Margin = new global::Avalonia.Thickness(16),
            Spacing = 12,
            Children =
            {
                new TextBlock { Text = "RuTracker login", FontSize = 18, FontWeight = FontWeight.SemiBold },
                status,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Children = { cancelButton, useButton },
                },
            },
        };

        var host = new Window
        {
            Title = "RuTracker login",
            Width = 440,
            Height = 190,
            CanResize = false,
            WindowStartupLocation = owner is null
                ? WindowStartupLocation.CenterScreen
                : WindowStartupLocation.CenterOwner,
            Content = panel,
        };

        var dialog = new NativeWebDialog
        {
            Title = "RuTracker — browser",
            CanUserResize = true,
            Source = new Uri(loginUrl),
        };
        dialog.Resize(960, 720);
        dialog.EnvironmentRequested += (_, e) =>
        {
            if (e is GtkWebViewEnvironmentRequestedEventArgs gtk)
            {
                gtk.BaseDataDirectory = webData;
                gtk.BaseCacheDirectory = Path.Combine(webData, "cache");
            }
        };

        var tcs = new TaskCompletionSource<(IReadOnlyList<Cookie> Cookies, string? UserAgent)?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var completing = false;
        var busy = false;
        IReadOnlyList<Cookie> latestCookies = [];
        string? latestUa = null;
        DispatcherTimer? timer = null;
        CancellationTokenRegistration ctr = default;

        void Complete((IReadOnlyList<Cookie> Cookies, string? UserAgent)? result)
        {
            if (completing)
                return;
            completing = true;
            timer?.Stop();
            timer = null;
            ctr.Dispose();

            try { dialog.Closing -= OnDialogClosing; } catch { }
            try { dialog.Close(); } catch { }
            try { dialog.Dispose(); } catch { }

            try
            {
                if (host.IsVisible)
                    host.Close();
            }
            catch
            {
            }

            tcs.TrySetResult(result);
        }

        void OnDialogClosing(object? sender, EventArgs e)
        {
            if (!completing)
                Dispatcher.UIThread.Post(() => Complete(null));
        }

        async Task RefreshCookiesAsync(bool autoAccept)
        {
            if (completing || busy)
                return;

            busy = true;
            try
            {
                latestUa = string.IsNullOrWhiteSpace(dialog.UserAgent) ? null : dialog.UserAgent;
                var raw = await RuTrackerGtkCookies.TryGetAllAsync(dialog, cancellationToken);
                var hostName = dialog.Source?.Host;
                latestCookies = raw
                    .Select(c => EnsureDomain(c, hostName))
                    .Where(IsRuTrackerCookie)
                    .ToList();

                var hasSession = HasSession(latestCookies);
                var leftLogin = LeftLoginPage(dialog.Source);
                status.Text = hasSession
                    ? $"Session detected ({latestCookies.Count} cookie(s)). Closing…"
                    : leftLogin
                        ? $"Browser left login page ({latestCookies.Count} cookie(s)). Click “Use session”."
                        : $"Waiting for login… ({latestCookies.Count} cookie(s))";

                if (autoAccept && hasSession)
                    Complete((latestCookies, latestUa));
            }
            catch (OperationCanceledException)
            {
                Complete(null);
            }
            catch (Exception ex)
            {
                RuTrackerLogError("Web login cookie poll failed", ex);
                status.Text = ex.Message;
            }
            finally
            {
                busy = false;
            }
        }

        useButton.Click += async (_, _) =>
        {
            status.Text = "Reading cookies from WebKit…";
            await RefreshCookiesAsync(autoAccept: false);
            if (completing)
                return;

            if (latestCookies.Count == 0)
            {
                status.Text = "No cookies yet. Finish Cloudflare + login, then try again.";
                return;
            }

            if (!HasSession(latestCookies))
            {
                status.Text =
                    $"No session cookie among: {string.Join(", ", latestCookies.Select(c => c.Name))}";
                return;
            }

            Complete((latestCookies, latestUa));
        };

        cancelButton.Click += (_, _) => Complete(null);
        host.Closing += (_, _) => Complete(null);

        dialog.NavigationCompleted += async (_, _) => await RefreshCookiesAsync(autoAccept: true);
        dialog.Closing += OnDialogClosing;

        timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        timer.Tick += async (_, _) => await RefreshCookiesAsync(autoAccept: true);
        timer.Start();

        if (cancellationToken.CanBeCanceled)
            ctr = cancellationToken.Register(() => Dispatcher.UIThread.Post(() => Complete(null)));

        if (owner is not null)
            host.Show(owner);
        else
            host.Show();

        if (owner is not null)
            dialog.Show(owner);
        else
            dialog.Show();

        host.Topmost = true;
        host.Activate();

        return await tcs.Task;
    }

    private static Cookie EnsureDomain(Cookie cookie, string? fallbackHost)
    {
        if (!string.IsNullOrWhiteSpace(cookie.Domain) || string.IsNullOrWhiteSpace(fallbackHost))
            return cookie;

        return new Cookie(cookie.Name, cookie.Value)
        {
            Domain = fallbackHost,
            Path = string.IsNullOrWhiteSpace(cookie.Path) ? "/" : cookie.Path,
            Secure = cookie.Secure,
            HttpOnly = cookie.HttpOnly,
            Expires = cookie.Expires,
        };
    }

    private static bool LeftLoginPage(Uri? source)
    {
        if (source is null)
            return false;
        var path = source.AbsolutePath;
        return !path.Contains("login.php", StringComparison.OrdinalIgnoreCase)
               && (source.Host.Contains("rutracker", StringComparison.OrdinalIgnoreCase)
                   || source.Host.Contains("rutrk", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasSession(IEnumerable<Cookie> cookies) =>
        cookies.Any(c =>
            RuTrackerCookieNames.IsSessionCookieName(c.Name)
            && !string.IsNullOrWhiteSpace(c.Value)
            && c.Value is not ("deleted" or "0"));

    private static bool IsRuTrackerCookie(Cookie cookie)
    {
        var domain = (cookie.Domain ?? string.Empty).Trim().TrimStart('.');
        if (string.IsNullOrEmpty(domain))
            return RuTrackerCookieNames.IsSessionCookieName(cookie.Name)
                   || cookie.Name.Contains("cf_", StringComparison.OrdinalIgnoreCase)
                   || cookie.Name.Contains("bb_", StringComparison.OrdinalIgnoreCase);

        return domain.Contains("rutracker", StringComparison.OrdinalIgnoreCase)
               || domain.Contains("rutrk", StringComparison.OrdinalIgnoreCase)
               || domain.EndsWith("cloudflare.com", StringComparison.OrdinalIgnoreCase);
    }

    internal static string RuTrackerMirrorsNormalize(string baseUrl) =>
        baseUrl.TrimEnd('/');

    internal static void RuTrackerLogError(string message, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[RuTracker] {message}: {ex.Message}");
    }

    private static void ApplyWebKitGtkWorkarounds()
    {
        if (!OperatingSystem.IsLinux())
            return;

        // Respect an explicitly configured backend (Wayland) instead of
        // forcing X11, which pushes the web view through XWayland and can
        // make rendering sluggish.
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GDK_BACKEND")))
            SetEnv("GDK_BACKEND", "x11");

        // Disable the DMA-BUF renderer only (safe on Fedora); never disable
        // WebKit compositing entirely — that makes the view software-only,
        // gray, and extremely slow (see commit e178578).
        SetEnv("WEBKIT_DISABLE_DMABUF_RENDERER", "1");
    }

    private static void SetEnv(string name, string value)
    {
        setenv(name, value, 1);
        Environment.SetEnvironmentVariable(name, value);
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int setenv(string name, string value, int overwrite);
}

/// <summary>
/// Cookie-name predicates shared between the host UI and the provider session logic.
/// Kept in App.Avalonia to avoid forcing plugin assemblies to reference the UI layer.
/// </summary>
internal static class RuTrackerCookieNames
{
    public static bool IsSessionCookieName(string name) =>
        name.Contains("bb_session", StringComparison.OrdinalIgnoreCase)
        || name.Contains("bb_data", StringComparison.OrdinalIgnoreCase)
        || name.Equals("bb_userid", StringComparison.OrdinalIgnoreCase)
        || name.Equals("bb_password", StringComparison.OrdinalIgnoreCase);
}