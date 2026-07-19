using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.RuTracker;

internal sealed class RuTrackerClient : IDisposable
{
    public const string DefaultBaseUrl = "https://rutracker.org";

    private readonly CookieContainer _cookies = new();
    private readonly HttpClient _http;
    private readonly RuTrackerSessionStore _session;
    private readonly string _baseUrl;
    private readonly Uri _baseUri;
    private bool _loggedIn;

    public RuTrackerClient(string? baseUrl = null, string? dataDirectory = null, HttpMessageHandler? handler = null)
    {
        RuTrackerEncoding.EnsureRegistered();
        _baseUrl = (baseUrl ?? DefaultBaseUrl).TrimEnd('/');
        _baseUri = new Uri(_baseUrl + "/");
        _session = new RuTrackerSessionStore(_baseUrl, dataDirectory);
        _session.LoadInto(_cookies);
        _loggedIn = RuTrackerSessionStore.HasSessionCookie(_cookies, _baseUri);
        RuTrackerLog.Info($"Client created. IsLoggedIn={_loggedIn}");

        var pipeline = handler ?? new HttpClientHandler
        {
            CookieContainer = _cookies,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.All,
            AllowAutoRedirect = true,
        };

        _http = new HttpClient(pipeline)
        {
            Timeout = TimeSpan.FromSeconds(10),
        };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        _http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8");
    }

    public void SetTimeout(TimeSpan timeout)
    {
        _http.Timeout = timeout <= TimeSpan.Zero ? Timeout.InfiniteTimeSpan : timeout;
    }

    public bool IsLoggedIn => _loggedIn && RuTrackerSessionStore.HasSessionCookie(_cookies, _baseUri);

    public async Task LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Username and password are required.");

        RuTrackerLog.Info($"Login attempt for user '{username.Trim()}'");

        using var content = RuTrackerEncoding.CreateFormUrlEncoded(new Dictionary<string, string>
        {
            ["login_username"] = username.Trim(),
            ["login_password"] = password,
            ["login"] = "Вход",
        });

        using var response = await _http.PostAsync($"{_baseUrl}/forum/login.php", content, cancellationToken);
        var body = await ReadHtmlAsync(response, cancellationToken);
        RuTrackerLog.Info($"Login HTTP {(int)response.StatusCode}, bodyLength={body.Length}, cookies={_cookies.Count}");

        if (body.Contains("неверн", StringComparison.OrdinalIgnoreCase)
            || body.Contains("incorrect", StringComparison.OrdinalIgnoreCase))
        {
            _loggedIn = false;
            throw new InvalidOperationException("RuTracker rejected the credentials.");
        }

        if (!RuTrackerSessionStore.HasSessionCookie(_cookies, _baseUri)
            && RuTrackerHtmlParser.LooksLikeLoginPage(body))
        {
            _loggedIn = false;
            throw new InvalidOperationException("RuTracker rejected the credentials.");
        }

        _session.Save(_cookies);
        _loggedIn = RuTrackerSessionStore.HasSessionCookie(_cookies, _baseUri);
        if (!_loggedIn)
        {
            RuTrackerLog.Error("Login response had no session cookie");
            throw new InvalidOperationException("RuTracker login did not establish a session.");
        }

        RuTrackerLog.Info("Login succeeded");
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

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"RuTracker search failed ({(int)response.StatusCode}).");

            if (RuTrackerHtmlParser.LooksLikeLoginPage(html)
                && html.Contains("login_password", StringComparison.OrdinalIgnoreCase))
            {
                _loggedIn = false;
                RuTrackerLog.Error("Search returned login page — session lost");
                throw new InvalidOperationException("RuTracker session expired. Please login again.");
            }

            var hits = RuTrackerHtmlParser.ParseSearchResults(html, _baseUrl);
            RuTrackerLog.Info($"Search parsed {hits.Count} hit(s)");
            _session.Save(_cookies);
            return hits;
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

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"RuTracker download failed ({(int)response.StatusCode}).");

        var head = Encoding.UTF8.GetString(bytes.AsSpan(0, Math.Min(bytes.Length, 64)));
        if (contentType.Contains("html", StringComparison.OrdinalIgnoreCase)
            || head.StartsWith("<", StringComparison.Ordinal)
            || head.Contains("login", StringComparison.OrdinalIgnoreCase))
        {
            var htmlHead = RuTrackerEncoding.Decode(bytes.AsSpan(0, Math.Min(bytes.Length, 4096)).ToArray(),
                response.Content.Headers.ContentType?.CharSet);
            if (RuTrackerHtmlParser.LooksLikeLoginPage(htmlHead))
            {
                _loggedIn = false;
                throw new InvalidOperationException("RuTracker requires login to download torrents.");
            }
        }

        if (bytes.Length < 16)
            throw new InvalidOperationException("RuTracker returned an empty torrent file.");

        _session.Save(_cookies);
        return bytes;
    }

    public void Dispose() => _http.Dispose();

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
