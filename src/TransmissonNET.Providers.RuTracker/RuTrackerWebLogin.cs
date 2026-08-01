using System.Net;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;

namespace TransmissonNET.Providers.RuTracker;

internal static class RuTrackerWebLogin
{
    public static async Task<(IReadOnlyList<Cookie> Cookies, string? UserAgent)?> ShowAsync(
        string baseUrl,
        string dataDirectory,
        Window? owner,
        CancellationToken cancellationToken = default)
    {
        ApplyWebKitGtkWorkarounds();

        var loginUrl = RuTrackerMirrors.NormalizeBaseUrl(baseUrl).TrimEnd('/') + "/forum/login.php";
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
            Margin = new Avalonia.Thickness(16),
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
                RuTrackerLog.Error("Web login cookie poll failed", ex);
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
        host.Closing += (_, e) =>
        {
            if (!completing)
            {
                e.Cancel = true;
                Complete(null);
            }
        };

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
            RuTrackerSessionStore.IsSessionCookieName(c.Name)
            && !string.IsNullOrWhiteSpace(c.Value)
            && c.Value is not ("deleted" or "0"));

    private static bool IsRuTrackerCookie(Cookie cookie)
    {
        var domain = (cookie.Domain ?? string.Empty).Trim().TrimStart('.');
        if (string.IsNullOrEmpty(domain))
            return RuTrackerSessionStore.IsSessionCookieName(cookie.Name)
                   || cookie.Name.Contains("cf_", StringComparison.OrdinalIgnoreCase)
                   || cookie.Name.Contains("bb_", StringComparison.OrdinalIgnoreCase);

        return domain.Contains("rutracker", StringComparison.OrdinalIgnoreCase)
               || domain.Contains("rutrk", StringComparison.OrdinalIgnoreCase)
               || domain.EndsWith("cloudflare.com", StringComparison.OrdinalIgnoreCase);
    }

    private static void ApplyWebKitGtkWorkarounds()
    {
        if (!OperatingSystem.IsLinux())
            return;

        SetEnv("GDK_BACKEND", "x11");
        SetEnv("WEBKIT_DISABLE_DMABUF_RENDERER", "1");
        SetEnv("WEBKIT_DISABLE_COMPOSITING_MODE", "1");
    }

    private static void SetEnv(string name, string value)
    {
        setenv(name, value, 1);
        Environment.SetEnvironmentVariable(name, value);
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int setenv(string name, string value, int overwrite);
}
