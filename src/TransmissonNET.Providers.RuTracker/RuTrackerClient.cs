using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.RuTracker;

internal sealed class RuTrackerCloudflareException : InvalidOperationException
{
    public RuTrackerCloudflareException(string message) : base(message)
    {
    }
}

internal sealed class RuTrackerClient : IDisposable
{
    public const string DefaultBaseUrl = "https://rutracker.org";

    private readonly string _dataDirectory;
    private readonly HttpMessageHandler? _handler;
    private CookieContainer _cookies = new();
    private HttpClient _http;
    private RuTrackerSessionStore _session;
    private string _baseUrl;
    private Uri _baseUri;
    private string _userAgent =
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
    private bool _loggedIn;

    public RuTrackerClient(string? baseUrl = null, string? dataDirectory = null, HttpMessageHandler? handler = null)
    {
        RuTrackerEncoding.EnsureRegistered();
        _dataDirectory = dataDirectory
                         ?? Path.Combine(
                             Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                             ".config",
                             "TransmissonNET",
                             "providers",
                             "rutracker");
        Directory.CreateDirectory(_dataDirectory);
        _handler = handler;
        _baseUrl = RuTrackerMirrors.NormalizeBaseUrl(baseUrl ?? DefaultBaseUrl).TrimEnd('/');
        _baseUri = new Uri(_baseUrl + "/");
        _session = new RuTrackerSessionStore(_baseUrl, _dataDirectory);
        _session.LoadInto(_cookies);
        if (!string.IsNullOrWhiteSpace(_session.UserAgent))
            _userAgent = _session.UserAgent;
        _loggedIn = RuTrackerSessionStore.HasSessionCookie(_cookies, _baseUri);
        _http = CreateHttp();
        RuTrackerLog.Info($"Client created. BaseUrl={_baseUrl}/, IsLoggedIn={_loggedIn}, ua={_userAgent}");
    }

    public string BaseUrl => _baseUrl + "/";

    public bool IsLoggedIn => _loggedIn && RuTrackerSessionStore.HasSessionCookie(_cookies, _baseUri);

    public void SetTimeout(TimeSpan timeout)
    {
        _http.Timeout = timeout <= TimeSpan.Zero ? Timeout.InfiniteTimeSpan : timeout;
    }

    public void SetBaseUrl(string baseUrl)
    {
        var normalized = RuTrackerMirrors.NormalizeBaseUrl(baseUrl).TrimEnd('/');
        if (string.Equals(normalized, _baseUrl, StringComparison.OrdinalIgnoreCase))
            return;

        _baseUrl = normalized;
        _baseUri = new Uri(_baseUrl + "/");
        _session = new RuTrackerSessionStore(_baseUrl, _dataDirectory);
        RebuildHttp(keepCookies: false);
        if (!string.IsNullOrWhiteSpace(_session.UserAgent))
            _userAgent = _session.UserAgent;
        _loggedIn = RuTrackerSessionStore.HasSessionCookie(_cookies, _baseUri);
        RuTrackerLog.Info($"BaseUrl switched to {_baseUrl}/, IsLoggedIn={_loggedIn}, ua={_userAgent}");
    }

    public void ImportWebSession(IEnumerable<Cookie> cookies, string? userAgent = null)
    {
        ArgumentNullException.ThrowIfNull(cookies);

        if (!string.IsNullOrWhiteSpace(userAgent))
            _userAgent = userAgent.Trim();

        _cookies = new CookieContainer();
        var imported = 0;
        foreach (var source in cookies)
        {
            if (string.IsNullOrWhiteSpace(source.Name) || string.IsNullOrWhiteSpace(source.Domain))
                continue;

            try
            {
                var domain = source.Domain.Trim().TrimStart('.');
                var cookie = new Cookie(source.Name, source.Value ?? string.Empty)
                {
                    Domain = domain,
                    Path = string.IsNullOrWhiteSpace(source.Path) ? "/" : source.Path,
                    Secure = source.Secure,
                    HttpOnly = source.HttpOnly,
                };
                if (source.Expires != DateTime.MinValue && source.Expires.Year > 2000)
                    cookie.Expires = source.Expires;

                _cookies.Add(new Uri($"https://{domain}/"), cookie);
                imported++;
            }
            catch (Exception ex)
            {
                RuTrackerLog.Error($"Skip imported cookie {source.Name}@{source.Domain}", ex);
            }
        }

        RebuildHttp(keepCookies: true);
        _session.Save(_cookies, _userAgent);
        _loggedIn = RuTrackerSessionStore.HasSessionCookie(_cookies, _baseUri);
        RuTrackerLog.Info($"Imported web session: cookies={imported}, loggedIn={_loggedIn}, ua={_userAgent}");
        if (!_loggedIn)
            throw new InvalidOperationException("Web login did not produce a RuTracker session cookie.");
    }

    public void ClearSession()
    {
        _session.Clear();
        _cookies = new CookieContainer();
        RebuildHttp(keepCookies: true);
        _loggedIn = false;
        RuTrackerLog.Info("Session cleared");
    }

    public async Task<IReadOnlyList<TorrentSearchHit>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var url = $"{_baseUrl}/forum/tracker.php?nm={Uri.EscapeDataString(query.Trim())}";
        RuTrackerLog.Info($"Search GET {url} (loggedIn={IsLoggedIn})");

        try
        {
            using var response = await _http.GetAsync(url, cancellationToken);
            var html = await ReadHtmlAsync(response, cancellationToken);
            RuTrackerLog.Info($"Search HTTP {(int)response.StatusCode}, bodyLength={html.Length}");
            ThrowIfCloudflare(response, html, "search");

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"RuTracker search failed ({(int)response.StatusCode}).");

            if (RuTrackerHtmlParser.LooksLikeLoginPage(html)
                && html.Contains("login_password", StringComparison.OrdinalIgnoreCase))
            {
                InvalidateSession("Search returned login page — session lost");
                throw new InvalidOperationException("RuTracker session expired. Please login again.");
            }

            var hits = RuTrackerHtmlParser.ParseSearchResults(html, _baseUrl);
            RuTrackerLog.Info($"Search parsed {hits.Count} hit(s)");
            _session.Save(_cookies, _userAgent);
            return hits;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            RuTrackerLog.Error("Search timed out", ex);
            throw new TimeoutException(
                "RuTracker search timed out. Try again or increase the provider timeout.",
                ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not OperationCanceledException)
        {
            RuTrackerLog.Error("Search failed", ex);
            throw;
        }
    }

    public async Task<byte[]> DownloadTorrentAsync(string hitId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hitId))
            throw new ArgumentException("Hit id is required.", nameof(hitId));

        var url = $"{_baseUrl}/forum/dl.php?t={Uri.EscapeDataString(hitId.Trim())}";
        RuTrackerLog.Info($"Download GET {url}");

        using var response = await _http.GetAsync(url, cancellationToken);
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        RuTrackerLog.Info($"Download HTTP {(int)response.StatusCode}, bytes={bytes.Length}, type={contentType}");

        var headHtml = RuTrackerEncoding.Decode(
            bytes.AsSpan(0, Math.Min(bytes.Length, 4096)).ToArray(),
            response.Content.Headers.ContentType?.CharSet);
        ThrowIfCloudflare(response, headHtml, "download");

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"RuTracker download failed ({(int)response.StatusCode}).");

        var head = Encoding.UTF8.GetString(bytes.AsSpan(0, Math.Min(bytes.Length, 64)));
        if (contentType.Contains("html", StringComparison.OrdinalIgnoreCase)
            || head.StartsWith("<", StringComparison.Ordinal)
            || head.Contains("login", StringComparison.OrdinalIgnoreCase))
        {
            if (RuTrackerHtmlParser.LooksLikeLoginPage(headHtml))
            {
                InvalidateSession("Download requires login");
                throw new InvalidOperationException("RuTracker requires login to download torrents.");
            }
        }

        if (bytes.Length < 16)
            throw new InvalidOperationException("RuTracker returned an empty torrent file.");

        _session.Save(_cookies, _userAgent);
        return bytes;
    }

    public void Dispose() => _http.Dispose();

    private void InvalidateSession(string reason)
    {
        RuTrackerLog.Error(reason);
        ClearSession();
    }

    private void ThrowIfCloudflare(HttpResponseMessage response, string? body, string action)
    {
        if (!RuTrackerHtmlParser.LooksLikeCloudflareChallenge(response, body))
            return;

        RuTrackerLog.Error($"Cloudflare challenge during {action} (HTTP {(int)response.StatusCode})");
        ClearSession();
        throw new RuTrackerCloudflareException(
            "RuTracker Cloudflare check expired. Please sign in again in the browser window.");
    }

    private void RebuildHttp(bool keepCookies)
    {
        var timeout = _http.Timeout;
        _http.Dispose();
        if (!keepCookies)
        {
            _cookies = new CookieContainer();
            _session.LoadInto(_cookies);
        }

        _http = CreateHttp();
        _http.Timeout = timeout;
    }

    private HttpClient CreateHttp()
    {
        var pipeline = _handler ?? new HttpClientHandler
        {
            CookieContainer = _cookies,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.All,
            AllowAutoRedirect = true,
        };

        var http = new HttpClient(pipeline) { Timeout = TimeSpan.FromSeconds(10) };
        http.DefaultRequestHeaders.UserAgent.Clear();
        http.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8");
        return http;
    }

    private static async Task<string> ReadHtmlAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var charset = response.Content.Headers.ContentType?.CharSet;
        try
        {
            return RuTrackerEncoding.Decode(bytes, charset);
        }
        catch (Exception ex)
        {
            RuTrackerLog.Error($"Decode failed charset='{charset}', falling back to windows-1251", ex);
            return RuTrackerEncoding.Default.GetString(bytes);
        }
    }
}
