using System.Net;
using System.Text.Json;

namespace TransmissonNET.Providers.RuTracker;

internal sealed class RuTrackerSessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _cookiesPath;
    private readonly Uri _baseUri;

    public RuTrackerSessionStore(string baseUrl, string? dataDirectory = null)
    {
        _baseUri = new Uri(baseUrl.TrimEnd('/') + "/");
        var root = dataDirectory
                   ?? Path.Combine(
                       Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                       ".config",
                       "TransmissonNET",
                       "providers",
                       "rutracker");
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
            RuTrackerLog.Info($"Cleared cookie file {_cookiesPath}");
        }
        catch (Exception ex)
        {
            RuTrackerLog.Error("Failed to clear cookies", ex);
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
        RuTrackerLog.Info($"Saved {cookies.Count} cookie(s) to {_cookiesPath}");
    }

    public int LoadInto(CookieContainer container)
    {
        if (!File.Exists(_cookiesPath))
        {
            RuTrackerLog.Info($"No cookie file at {_cookiesPath}");
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
                    RuTrackerLog.Error($"Skip cookie {c.Name}@{c.Domain}", ex);
                }
            }

            RuTrackerLog.Info($"Loaded {loaded} cookie(s) from {_cookiesPath}");
            return loaded;
        }
        catch (Exception ex)
        {
            RuTrackerLog.Error("Failed to load cookies", ex);
            return 0;
        }
    }

    public static bool HasSessionCookie(CookieContainer container, Uri baseUri)
    {
        return EnumerateCookies(container)
            .Any(c =>
            {
                if (!IsSessionCookieName(c.Name))
                    return false;
                if (string.IsNullOrWhiteSpace(c.Value) || c.Value is "deleted" or "0")
                    return false;
                return true;
            });
    }

    public static IEnumerable<Cookie> EnumerateCookies(CookieContainer container)
    {
        foreach (Cookie cookie in container.GetAllCookies())
            yield return cookie;
    }

    private static bool IsSessionCookieName(string name) =>
        name.Contains("bb_session", StringComparison.OrdinalIgnoreCase)
        || name.Contains("bb_data", StringComparison.OrdinalIgnoreCase)
        || name.Equals("bb_userid", StringComparison.OrdinalIgnoreCase)
        || name.Equals("bb_password", StringComparison.OrdinalIgnoreCase);

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
