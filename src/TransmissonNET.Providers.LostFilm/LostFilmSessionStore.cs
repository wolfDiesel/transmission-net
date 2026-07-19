using System.Net;
using System.Text.Json;

namespace TransmissonNET.Providers.LostFilm;

internal sealed class LostFilmSessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _cookiesPath;
    private readonly Uri _baseUri;

    public LostFilmSessionStore(string baseUrl, string? dataDirectory = null)
    {
        _baseUri = new Uri(LostFilmMirrors.NormalizeBaseUrl(baseUrl));
        var root = dataDirectory
                   ?? Path.Combine(
                       Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                       ".config",
                       "TransmissonNET",
                       "providers",
                       "lostfilm");
        Directory.CreateDirectory(root);
        _cookiesPath = Path.Combine(root, "cookies.json");
    }

    public string CookiesPath => _cookiesPath;

    public void Clear()
    {
        try
        {
            if (File.Exists(_cookiesPath))
                File.Delete(_cookiesPath);
            LostFilmLog.Info($"Cleared cookie file {_cookiesPath}");
        }
        catch (Exception ex)
        {
            LostFilmLog.Error("Failed to clear cookies", ex);
        }
    }

    public void Save(CookieContainer container)
    {
        var cookies = EnumerateCookies(container)
            .Select(c => new StoredCookie(
                c.Name,
                c.Value,
                NormalizeDomain(c.Domain),
                string.IsNullOrWhiteSpace(c.Path) ? "/" : c.Path,
                c.Expires == DateTime.MinValue || c.Expires.Year < 2000 ? null : c.Expires.ToUniversalTime(),
                c.Secure,
                c.HttpOnly))
            .GroupBy(c => (c.Name, c.Domain, c.Path), StringTupleComparer.Instance)
            .Select(g => g.Last())
            .ToList();

        File.WriteAllText(_cookiesPath, JsonSerializer.Serialize(cookies, JsonOptions));
        LostFilmLog.Info($"Saved {cookies.Count} cookie(s) to {_cookiesPath}");
    }

    public int LoadInto(CookieContainer container)
    {
        if (!File.Exists(_cookiesPath))
        {
            LostFilmLog.Info($"No cookie file at {_cookiesPath}");
            return 0;
        }

        try
        {
            var json = File.ReadAllText(_cookiesPath);
            var cookies = JsonSerializer.Deserialize<List<StoredCookie>>(json);
            if (cookies is null || cookies.Count == 0)
                return 0;

            var loaded = 0;
            foreach (var c in cookies)
            {
                if (string.IsNullOrWhiteSpace(c.Name) || string.IsNullOrWhiteSpace(c.Domain))
                    continue;

                if (c.Expires is { } expires && expires.ToUniversalTime() < DateTime.UtcNow)
                    continue;

                try
                {
                    var cookie = new Cookie(c.Name, c.Value ?? string.Empty)
                    {
                        Domain = NormalizeDomain(c.Domain),
                        Path = string.IsNullOrWhiteSpace(c.Path) ? "/" : c.Path,
                        Secure = c.Secure,
                        HttpOnly = c.HttpOnly,
                    };
                    if (c.Expires is { } exp)
                        cookie.Expires = exp.ToUniversalTime();

                    container.Add(_baseUri, cookie);
                    loaded++;
                }
                catch (Exception ex)
                {
                    LostFilmLog.Error($"Skip cookie {c.Name}@{c.Domain}", ex);
                }
            }

            LostFilmLog.Info($"Loaded {loaded} cookie(s) from {_cookiesPath}");
            return loaded;
        }
        catch (Exception ex)
        {
            LostFilmLog.Error("Failed to load cookies", ex);
            return 0;
        }
    }

    public void SetSessionCookie(CookieContainer container, string lfSession)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lfSession);
        var cookie = new Cookie("lf_session", lfSession.Trim())
        {
            Domain = _baseUri.Host,
            Path = "/",
            Secure = _baseUri.Scheme == Uri.UriSchemeHttps,
        };
        container.Add(_baseUri, cookie);
        Save(container);
    }

    public static bool HasSessionCookie(CookieContainer container)
    {
        return EnumerateCookies(container)
            .Any(c =>
                c.Name.Equals("lf_session", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(c.Value)
                && c.Value is not ("deleted" or "0"));
    }

    public static IEnumerable<Cookie> EnumerateCookies(CookieContainer container)
    {
        foreach (Cookie cookie in container.GetAllCookies())
            yield return cookie;
    }

    private static string NormalizeDomain(string domain)
    {
        var d = domain.Trim();
        if (d.StartsWith('.'))
            d = d[1..];
        return d;
    }

    private sealed record StoredCookie(
        string Name,
        string? Value,
        string Domain,
        string? Path,
        DateTime? Expires,
        bool Secure,
        bool HttpOnly);

    private sealed class StringTupleComparer : IEqualityComparer<(string Name, string Domain, string? Path)>
    {
        public static readonly StringTupleComparer Instance = new();

        public bool Equals((string Name, string Domain, string? Path) x, (string Name, string Domain, string? Path) y) =>
            string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.Domain, y.Domain, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.Path, y.Path, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string Name, string Domain, string? Path) obj) =>
            HashCode.Combine(
                obj.Name.ToLowerInvariant(),
                obj.Domain.ToLowerInvariant(),
                obj.Path?.ToLowerInvariant());
    }
}
