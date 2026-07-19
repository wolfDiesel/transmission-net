namespace TransmissonNET.Providers.RuTracker;

internal static class RuTrackerMirrors
{
    public const string DefaultBaseUrl = "https://rutracker.org/";

    public static readonly string[] FallbackUrls =
    [
        "https://rutracker.org/",
        "https://rutracker.net/",
        "https://rutracker.nl/",
    ];

    public static string NormalizeBaseUrl(string? url)
    {
        var value = string.IsNullOrWhiteSpace(url) ? DefaultBaseUrl : url.Trim();
        if (!value.EndsWith('/'))
            value += "/";
        return value;
    }
}
