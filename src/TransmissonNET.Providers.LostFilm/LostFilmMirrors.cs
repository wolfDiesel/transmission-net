namespace TransmissonNET.Providers.LostFilm;

internal static class LostFilmMirrors
{
    public const string DefaultBaseUrl = "https://www.lostfilm.download/";

    public static readonly string[] FallbackUrls =
    [
        "https://www.lostfilm.download/",
        "https://www.lostfilm.today/",
        "https://www.lostfilmtv2.site/",
        "https://www.lostfilm.top/",
        "https://www.lostfilm.run/",
        "https://www.lostfilm.uno/",
        "https://www.lostfilmtv3.site/",
        "https://www.lostfilm.tv/",
    ];

    public static string NormalizeBaseUrl(string? url)
    {
        var value = string.IsNullOrWhiteSpace(url) ? DefaultBaseUrl : url.Trim();
        if (!value.EndsWith('/'))
            value += "/";
        return value;
    }

    public static IReadOnlyList<string> Candidates(string? preferred)
    {
        var list = new List<string>();
        var primary = NormalizeBaseUrl(preferred);
        list.Add(primary);
        foreach (var mirror in FallbackUrls)
        {
            var n = NormalizeBaseUrl(mirror);
            if (!list.Contains(n, StringComparer.OrdinalIgnoreCase))
                list.Add(n);
        }

        return list;
    }
}
