namespace TransmissonNET.Providers.Kinozal;

internal static class KinozalMirrors
{
    public const string DefaultBaseUrl = "https://kinozal.me/";

    public static readonly string[] FallbackUrls =
    [
        "https://kinozal.me/",
        "https://kinozal.tv/",
        "https://kinozal.guru/",
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
