using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.RuTracker;

internal static class RuTrackerHtmlParser
{
    private static readonly Regex TopicIdRegex = new(
        @"[?&]t=(\d+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SizeTokenRegex = new(
        @"([\d]+(?:[.,]\d+)?)\s*(KB|MB|GB|TB|B|КБ|МБ|ГБ|ТБ)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static IReadOnlyList<TorrentSearchHit> ParseSearchResults(string html, string baseUrl)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var hits = new List<TorrentSearchHit>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        var rows = doc.DocumentNode.SelectNodes("//tr[@data-topic_id]")
                    ?? doc.DocumentNode.SelectNodes("//table[contains(@class,'forumline') or @id='tor-tbl']//tr[td]");

        if (rows is null)
            return hits;

        foreach (var row in rows)
        {
            var topicId = row.GetAttributeValue("data-topic_id", null)
                          ?? ExtractTopicId(row.InnerHtml);
            if (string.IsNullOrWhiteSpace(topicId) || !seen.Add(topicId))
                continue;

            var titleLink = row.SelectSingleNode(".//a[contains(@href,'viewtopic.php') and contains(@href,'t=')]")
                            ?? row.SelectSingleNode(".//a[contains(@class,'torTopic') or contains(@class,'tt-text')]");
            if (titleLink is null)
                continue;

            var title = HtmlEntity.DeEntitize(titleLink.InnerText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
                continue;

            var href = titleLink.GetAttributeValue("href", string.Empty);
            var detailUrl = ToAbsoluteUrl(baseUrl, href);

            long? sizeBytes = null;
            var sizeNode = row.SelectSingleNode(".//td[contains(@class,'tor-size')]")
                           ?? row.SelectSingleNode(".//*[contains(@class,'tor-size')]");
            if (sizeNode is not null)
                sizeBytes = ParseSizeBytes(HtmlEntity.DeEntitize(sizeNode.InnerText));

            hits.Add(new TorrentSearchHit(topicId, title, sizeBytes, detailUrl));
        }

        return hits;
    }

    public static long? ParseSizeBytes(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var match = SizeTokenRegex.Match(text.Replace('\u00A0', ' '));
        if (!match.Success)
            return null;

        var numberText = match.Groups[1].Value.Replace(',', '.');
        if (!double.TryParse(numberText, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return null;

        var unit = match.Groups[2].Value.ToUpperInvariant();
        var multiplier = unit switch
        {
            "B" => 1d,
            "KB" or "КБ" => 1024d,
            "MB" or "МБ" => 1024d * 1024d,
            "GB" or "ГБ" => 1024d * 1024d * 1024d,
            "TB" or "ТБ" => 1024d * 1024d * 1024d * 1024d,
            _ => 1d,
        };

        return (long)Math.Round(value * multiplier);
    }

    public static bool LooksLikeLoginPage(string html) =>
        html.Contains("login_username", StringComparison.OrdinalIgnoreCase)
        || html.Contains("name=\"login_password\"", StringComparison.OrdinalIgnoreCase);

    private static string? ExtractTopicId(string html)
    {
        var match = TopicIdRegex.Match(html);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string? ToAbsoluteUrl(string baseUrl, string href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return null;
        if (Uri.TryCreate(href, UriKind.Absolute, out var absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            return absolute.ToString();

        var siteRoot = new Uri(baseUrl.TrimEnd('/') + "/");
        if (href.StartsWith('/'))
        {
            if (Uri.TryCreate(siteRoot, href, out var fromRoot))
                return fromRoot.ToString();
            return href;
        }

        var trimmed = baseUrl.TrimEnd('/');
        var forumBase = trimmed.EndsWith("/forum", StringComparison.OrdinalIgnoreCase)
            ? new Uri(trimmed + "/")
            : new Uri(trimmed + "/forum/");
        if (Uri.TryCreate(forumBase, href, out var combined))
            return combined.ToString();
        return href;
    }
}
