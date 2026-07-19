using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.Kinozal;

internal static class KinozalHtmlParser
{
    private static readonly Regex IdRegex = new(
        @"[?&]id=(\d+)",
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
        var baseNormalized = KinozalMirrors.NormalizeBaseUrl(baseUrl);

        var rows = doc.DocumentNode.SelectNodes("//tr[.//td[contains(@class,'bt')] and .//a[contains(@href,'details.php')]]")
                   ?? doc.DocumentNode.SelectNodes("//tr[.//a[contains(@href,'details.php?id=')]]");

        if (rows is null)
            return hits;

        foreach (var row in rows)
        {
            var link = row.SelectSingleNode(".//td[contains(@class,'nam')]//a[contains(@href,'details.php')]")
                       ?? row.SelectSingleNode(".//a[contains(@href,'details.php?id=')]");
            if (link is null)
                continue;

            var href = link.GetAttributeValue("href", string.Empty);
            var id = ExtractId(href);
            if (string.IsNullOrWhiteSpace(id) || !seen.Add(id))
                continue;

            var title = HtmlEntity.DeEntitize(link.InnerText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
                continue;

            long? sizeBytes = null;
            foreach (var cell in row.SelectNodes("./td[contains(@class,'s')]") ?? Enumerable.Empty<HtmlNode>())
            {
                var parsed = ParseSizeBytes(HtmlEntity.DeEntitize(cell.InnerText));
                if (parsed is not null)
                {
                    sizeBytes = parsed;
                    break;
                }
            }

            hits.Add(new TorrentSearchHit(
                id,
                title,
                sizeBytes,
                ToAbsoluteUrl(baseNormalized, href)));
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
        html.Contains("takelogin.php", StringComparison.OrdinalIgnoreCase)
        && html.Contains("name=\"password\"", StringComparison.OrdinalIgnoreCase)
        && !html.Contains("logout.php?hash4u=", StringComparison.OrdinalIgnoreCase);

    public static bool LooksLikeLoggedIn(string html) =>
        html.Contains("logout.php?hash4u=", StringComparison.OrdinalIgnoreCase);

    private static string? ExtractId(string href)
    {
        var match = IdRegex.Match(href);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string ToAbsoluteUrl(string baseUrl, string href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return baseUrl;
        if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return href;
        return new Uri(new Uri(baseUrl), href).ToString();
    }
}
