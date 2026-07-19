using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.LostFilm;

internal sealed class LostFilmClient : IDisposable
{
    private const int MaxEpisodesPerSeries = 50;

    private CookieContainer _cookies = new();
    private readonly string _dataDirectory;
    private HttpClient _http;
    private LostFilmSessionStore _session;
    private string _baseUrl;
    private Uri _baseUri;
    private bool _loggedIn;

    public LostFilmClient(string? baseUrl = null, string? dataDirectory = null, HttpMessageHandler? handler = null)
    {
        _dataDirectory = dataDirectory
                         ?? Path.Combine(
                             Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                             ".config",
                             "TransmissonNET",
                             "providers",
                             "lostfilm");
        Directory.CreateDirectory(_dataDirectory);
        _baseUrl = LostFilmMirrors.NormalizeBaseUrl(baseUrl);
        _baseUri = new Uri(_baseUrl);
        _session = new LostFilmSessionStore(_baseUrl, _dataDirectory);
        _session.LoadInto(_cookies);
        _loggedIn = LostFilmSessionStore.HasSessionCookie(_cookies);
        _http = CreateHttp(handler);
        LostFilmLog.Info($"Client created. BaseUrl={_baseUrl}, IsLoggedIn={_loggedIn}");
    }

    public string BaseUrl => _baseUrl;

    public bool IsLoggedIn => _loggedIn && LostFilmSessionStore.HasSessionCookie(_cookies);

    public void SetTimeout(TimeSpan timeout)
    {
        _http.Timeout = timeout <= TimeSpan.Zero ? Timeout.InfiniteTimeSpan : timeout;
    }

    public void SetBaseUrl(string baseUrl)
    {
        var normalized = LostFilmMirrors.NormalizeBaseUrl(baseUrl);
        if (string.Equals(normalized, _baseUrl, StringComparison.OrdinalIgnoreCase))
            return;

        _baseUrl = normalized;
        _baseUri = new Uri(_baseUrl);
        _session = new LostFilmSessionStore(_baseUrl, _dataDirectory);
        RebuildHttp();
        _loggedIn = LostFilmSessionStore.HasSessionCookie(_cookies);
        LostFilmLog.Info($"BaseUrl switched to {_baseUrl}, IsLoggedIn={_loggedIn}");
    }

    public async Task EnsureMirrorAsync(CancellationToken cancellationToken = default)
    {
        foreach (var candidate in LostFilmMirrors.Candidates(_baseUrl))
        {
            try
            {
                using var response = await _http.GetAsync(candidate, cancellationToken);
                if ((int)response.StatusCode is >= 200 and < 500)
                {
                    if (!string.Equals(candidate, _baseUrl, StringComparison.OrdinalIgnoreCase))
                        SetBaseUrl(candidate);
                    LostFilmLog.Info($"Mirror OK: {candidate} ({(int)response.StatusCode})");
                    return;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LostFilmLog.Error($"Mirror failed: {candidate}", ex);
            }
        }

        throw new InvalidOperationException("No reachable LostFilm mirror.");
    }

    public async Task LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Email and password are required.");

        LostFilmLog.Info($"Login attempt for '{email.Trim()}'");
        await EnsureMirrorAsync(cancellationToken);

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["act"] = "users",
            ["type"] = "login",
            ["mail"] = email.Trim(),
            ["pass"] = password,
            ["rem"] = "1",
        });

        using var response = await _http.PostAsync($"{_baseUrl}ajaxik.php", content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        LostFilmLog.Info($"Login HTTP {(int)response.StatusCode}, bodyLength={body.Length}");

        if (!body.Contains("\"success\":true", StringComparison.OrdinalIgnoreCase))
        {
            _loggedIn = false;
            if (body.Contains("\"error\":3", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("LostFilm rejected the credentials.");
            if (body.Contains("captcha", StringComparison.OrdinalIgnoreCase)
                || body.Contains("\"error\":1", StringComparison.OrdinalIgnoreCase)
                || body.Contains("\"error\":2", StringComparison.OrdinalIgnoreCase)
                || body.Contains("\"error\":4", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("LostFilm requires captcha. Paste lf_session cookie instead.");

            throw new InvalidOperationException("LostFilm login failed.");
        }

        _session.Save(_cookies);
        _loggedIn = LostFilmSessionStore.HasSessionCookie(_cookies);
        if (!_loggedIn)
            throw new InvalidOperationException("LostFilm login did not establish lf_session.");

        LostFilmLog.Info("Login succeeded");
    }

    public void LoginWithSessionCookie(string lfSession)
    {
        _session.SetSessionCookie(_cookies, lfSession);
        _loggedIn = true;
        LostFilmLog.Info("Session cookie applied");
    }

    public void Logout()
    {
        _session.Clear();
        _cookies = new CookieContainer();
        RebuildHttp();
        _loggedIn = false;
        LostFilmLog.Info("Logged out");
    }

    public async Task<IReadOnlyList<TorrentSearchHit>> SearchAsync(
        string query,
        int maxSeriesExpand,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        await EnsureMirrorAsync(cancellationToken);
        var url = $"{_baseUrl}search/?q={Uri.EscapeDataString(query.Trim())}";
        LostFilmLog.Info($"Search GET {url}");

        using var response = await _http.GetAsync(url, cancellationToken);
        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"LostFilm search failed ({(int)response.StatusCode}).");

        var series = LostFilmHtmlParser.ParseSearchSeries(html)
            .Take(Math.Clamp(maxSeriesExpand, 1, 20))
            .ToList();
        LostFilmLog.Info($"Search found {series.Count} series to expand");

        var hits = new List<TorrentSearchHit>();
        foreach (var item in series)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var seasonsUrl = $"{_baseUrl}series/{item.Slug}/seasons";
            using var seasonsResponse = await _http.GetAsync(seasonsUrl, cancellationToken);
            var seasonsHtml = await seasonsResponse.Content.ReadAsStringAsync(cancellationToken);
            if (!seasonsResponse.IsSuccessStatusCode)
            {
                LostFilmLog.Error($"Seasons page failed for {item.Slug}: {(int)seasonsResponse.StatusCode}");
                continue;
            }

            var episodes = LostFilmHtmlParser.ParseSeasonEpisodes(
                seasonsHtml,
                item.Title,
                item.Slug,
                _baseUrl,
                MaxEpisodesPerSeries);
            hits.AddRange(episodes);
        }

        LostFilmLog.Info($"Search parsed {hits.Count} episode hit(s)");
        _session.Save(_cookies);
        return hits;
    }

    public async Task<byte[]> DownloadTorrentAsync(
        string hitId,
        string preferredQuality,
        CancellationToken cancellationToken = default)
    {
        var (seriesId, season, episode) = LostFilmHtmlParser.ParseCode(hitId);
        await EnsureMirrorAsync(cancellationToken);

        var redirectUrl =
            $"{_baseUrl}v_search.php?c={seriesId}&s={season}&e={episode}";
        LostFilmLog.Info($"Download redirect GET {redirectUrl}");

        using var redirectResponse = await _http.GetAsync(redirectUrl, cancellationToken);
        var redirectHtml = await redirectResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!redirectResponse.IsSuccessStatusCode)
            throw new InvalidOperationException($"LostFilm v_search failed ({(int)redirectResponse.StatusCode}).");

        var downloadPageUrl = LostFilmHtmlParser.ParseDownloadRedirectUrl(redirectHtml, _baseUrl);
        if (string.IsNullOrWhiteSpace(downloadPageUrl))
        {
            _loggedIn = LostFilmSessionStore.HasSessionCookie(_cookies);
            throw new InvalidOperationException(
                "LostFilm download redirect missing. Login or paste a valid lf_session cookie.");
        }

        LostFilmLog.Info($"Download page GET {downloadPageUrl}");
        using var pageResponse = await _http.GetAsync(downloadPageUrl, cancellationToken);
        var pageHtml = await pageResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!pageResponse.IsSuccessStatusCode)
            throw new InvalidOperationException($"LostFilm download page failed ({(int)pageResponse.StatusCode}).");

        var qualities = LostFilmHtmlParser.ParseQualityLinks(pageHtml);
        var torrentUrl = LostFilmHtmlParser.SelectQualityUrl(qualities, preferredQuality);
        if (string.IsNullOrWhiteSpace(torrentUrl))
            throw new InvalidOperationException("LostFilm download page has no torrent links.");

        LostFilmLog.Info($"Torrent GET {torrentUrl} (preferred={preferredQuality})");
        using var torrentResponse = await _http.GetAsync(torrentUrl, cancellationToken);
        var bytes = await torrentResponse.Content.ReadAsByteArrayAsync(cancellationToken);
        if (!torrentResponse.IsSuccessStatusCode)
            throw new InvalidOperationException($"LostFilm torrent download failed ({(int)torrentResponse.StatusCode}).");

        var head = Encoding.UTF8.GetString(bytes.AsSpan(0, Math.Min(bytes.Length, 64)));
        if (head.StartsWith('<') || head.Contains("login", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("LostFilm returned HTML instead of a torrent file.");

        if (bytes.Length < 16)
            throw new InvalidOperationException("LostFilm returned an empty torrent file.");

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

        var http = new HttpClient(pipeline)
        {
            Timeout = TimeSpan.FromSeconds(10),
        };
        http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8");
        return http;
    }
}
