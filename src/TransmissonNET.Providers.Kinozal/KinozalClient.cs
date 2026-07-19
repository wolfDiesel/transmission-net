using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.Kinozal;

internal sealed class KinozalClient : IDisposable
{
    private CookieContainer _cookies = new();
    private readonly string _dataDirectory;
    private HttpClient _http;
    private KinozalSessionStore _session;
    private string _baseUrl;
    private bool _loggedIn;

    public KinozalClient(string? baseUrl = null, string? dataDirectory = null, HttpMessageHandler? handler = null)
    {
        _dataDirectory = dataDirectory
                         ?? Path.Combine(
                             Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                             ".config",
                             "TransmissonNET",
                             "providers",
                             "kinozal");
        Directory.CreateDirectory(_dataDirectory);
        _baseUrl = KinozalMirrors.NormalizeBaseUrl(baseUrl);
        _session = new KinozalSessionStore(_baseUrl, _dataDirectory);
        _session.LoadInto(_cookies);
        _loggedIn = KinozalSessionStore.HasSessionCookie(_cookies);
        _http = CreateHttp(handler);
        KinozalLog.Info($"Client created. BaseUrl={_baseUrl}, IsLoggedIn={_loggedIn}");
    }

    public string BaseUrl => _baseUrl;

    public bool IsLoggedIn => _loggedIn && KinozalSessionStore.HasSessionCookie(_cookies);

    public void SetTimeout(TimeSpan timeout) =>
        _http.Timeout = timeout <= TimeSpan.Zero ? Timeout.InfiniteTimeSpan : timeout;

    public void SetBaseUrl(string baseUrl)
    {
        var normalized = KinozalMirrors.NormalizeBaseUrl(baseUrl);
        if (string.Equals(normalized, _baseUrl, StringComparison.OrdinalIgnoreCase))
            return;

        _baseUrl = normalized;
        _session = new KinozalSessionStore(_baseUrl, _dataDirectory);
        RebuildHttp();
        _loggedIn = KinozalSessionStore.HasSessionCookie(_cookies);
        KinozalLog.Info($"BaseUrl switched to {_baseUrl}, IsLoggedIn={_loggedIn}");
    }

    public async Task EnsureMirrorAsync(CancellationToken cancellationToken = default)
    {
        foreach (var candidate in KinozalMirrors.Candidates(_baseUrl))
        {
            try
            {
                using var response = await _http.GetAsync(candidate, cancellationToken);
                if ((int)response.StatusCode is >= 200 and < 500)
                {
                    if (!string.Equals(candidate, _baseUrl, StringComparison.OrdinalIgnoreCase))
                        SetBaseUrl(candidate);
                    KinozalLog.Info($"Mirror OK: {candidate} ({(int)response.StatusCode})");
                    return;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                KinozalLog.Error($"Mirror failed: {candidate}", ex);
            }
        }

        throw new InvalidOperationException("No reachable Kinozal mirror.");
    }

    public async Task LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Username and password are required.");

        KinozalLog.Info($"Login attempt for '{username.Trim()}'");
        await EnsureMirrorAsync(cancellationToken);

        using var content = KinozalEncoding.CreateFormUrlEncoded(new Dictionary<string, string>
        {
            ["username"] = username.Trim(),
            ["password"] = password,
        });

        using var response = await _http.PostAsync($"{_baseUrl}takelogin.php", content, cancellationToken);
        var body = await ReadHtmlAsync(response, cancellationToken);
        KinozalLog.Info($"Login HTTP {(int)response.StatusCode}, bodyLength={body.Length}");

        if (body.Contains("div class=\"red\"", StringComparison.OrdinalIgnoreCase)
            || body.Contains("неверн", StringComparison.OrdinalIgnoreCase)
            || body.Contains("incorrect", StringComparison.OrdinalIgnoreCase))
        {
            _loggedIn = false;
            throw new InvalidOperationException("Kinozal rejected the credentials.");
        }

        _session.Save(_cookies);
        _loggedIn = KinozalSessionStore.HasSessionCookie(_cookies)
                    || KinozalHtmlParser.LooksLikeLoggedIn(body);

        if (!_loggedIn)
        {
            using var probe = await _http.GetAsync($"{_baseUrl}my.php", cancellationToken);
            var probeHtml = await ReadHtmlAsync(probe, cancellationToken);
            _loggedIn = KinozalHtmlParser.LooksLikeLoggedIn(probeHtml)
                        || KinozalSessionStore.HasSessionCookie(_cookies);
        }

        if (!_loggedIn)
            throw new InvalidOperationException("Kinozal login did not establish a session.");

        KinozalLog.Info("Login succeeded");
    }

    public void Logout()
    {
        _session.Clear();
        _cookies = new CookieContainer();
        RebuildHttp();
        _loggedIn = false;
        KinozalLog.Info("Logged out");
    }

    public async Task<IReadOnlyList<TorrentSearchHit>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        await EnsureMirrorAsync(cancellationToken);
        var url =
            $"{_baseUrl}browse.php?s={Uri.EscapeDataString(query.Trim())}&c=0&g=0&v=0&d=0&w=0&t=0&f=0";
        KinozalLog.Info($"Search GET {url}");

        using var response = await _http.GetAsync(url, cancellationToken);
        var html = await ReadHtmlAsync(response, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Kinozal search failed ({(int)response.StatusCode}).");

        if (KinozalHtmlParser.LooksLikeLoginPage(html))
        {
            _loggedIn = false;
            throw new InvalidOperationException("Kinozal session expired. Please login again.");
        }

        var hits = KinozalHtmlParser.ParseSearchResults(html, _baseUrl);
        KinozalLog.Info($"Search parsed {hits.Count} hit(s)");
        _session.Save(_cookies);
        return hits;
    }

    public async Task<byte[]> DownloadTorrentAsync(string hitId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hitId))
            throw new ArgumentException("Hit id is required.", nameof(hitId));

        await EnsureMirrorAsync(cancellationToken);
        var url = $"{_baseUrl}download.php?id={Uri.EscapeDataString(hitId.Trim())}";
        KinozalLog.Info($"Download GET {url}");

        using var response = await _http.GetAsync(url, cancellationToken);
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        KinozalLog.Info($"Download HTTP {(int)response.StatusCode}, bytes={bytes.Length}, type={contentType}");

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Kinozal download failed ({(int)response.StatusCode}).");

        var head = Encoding.UTF8.GetString(bytes.AsSpan(0, Math.Min(bytes.Length, 64)));
        if (contentType.Contains("html", StringComparison.OrdinalIgnoreCase)
            || head.StartsWith('<')
            || head.Contains("login", StringComparison.OrdinalIgnoreCase))
        {
            var htmlHead = KinozalEncoding.Decode(
                bytes.AsSpan(0, Math.Min(bytes.Length, 4096)).ToArray(),
                response.Content.Headers.ContentType?.CharSet);
            if (KinozalHtmlParser.LooksLikeLoginPage(htmlHead))
            {
                _loggedIn = false;
                throw new InvalidOperationException("Kinozal requires login to download torrents.");
            }

            throw new InvalidOperationException(
                "Kinozal returned HTML instead of a torrent (daily download limit?).");
        }

        if (bytes.Length < 16)
            throw new InvalidOperationException("Kinozal returned an empty torrent file.");

        _session.Save(_cookies);
        return bytes;
    }

    public void Dispose() => _http.Dispose();

    private void RebuildHttp()
    {
        _http.Dispose();
        _http = CreateHttp(null);
    }

    private HttpClient CreateHttp(HttpMessageHandler? handler)
    {
        var pipeline = handler ?? new HttpClientHandler
        {
            CookieContainer = _cookies,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.All,
            AllowAutoRedirect = true,
        };

        var http = new HttpClient(pipeline) { Timeout = TimeSpan.FromSeconds(10) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8");
        return http;
    }

    private static async Task<string> ReadHtmlAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return KinozalEncoding.Decode(bytes, response.Content.Headers.ContentType?.CharSet);
    }
}
