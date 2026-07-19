using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Providers.LostFilm;

internal static class LostFilmHtmlParser
{
    private static readonly Regex CodeRegex = new(
        @"^(?<c>\d+)-(?<s>\d+)-(?<e>\d+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex MetaRefreshRegex = new(
        @"url=(?<url>.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static IReadOnlyList<LostFilmSeriesHit> ParseSearchSeries(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var results = new List<LostFilmSeriesHit>();

        foreach (var row in doc.DocumentNode.SelectNodes("//div[contains(@class,'row-search')]") ?? Enumerable.Empty<HtmlNode>())
        {
            var link = row.SelectSingleNode(".//a[contains(@href,'/series/')]");
            if (link is null)
                continue;

            var href = link.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrWhiteSpace(href) || href.Contains("/movies/", StringComparison.OrdinalIgnoreCase))
                continue;

            var slug = ExtractSeriesSlug(href);
            if (string.IsNullOrWhiteSpace(slug))
                continue;

            var nameEn = row.SelectSingleNode(".//div[contains(@class,'name-en')]")?.InnerText?.Trim();
            var nameRu = row.SelectSingleNode(".//div[contains(@class,'name-ru')]")?.InnerText?.Trim();
            var title = string.IsNullOrWhiteSpace(nameEn) ? (nameRu ?? slug) : nameEn;
            if (!string.IsNullOrWhiteSpace(nameRu) && !string.Equals(nameRu, title, StringComparison.Ordinal))
                title = $"{nameRu} ({title})";

            results.Add(new LostFilmSeriesHit(slug, title, $"/series/{slug}"));
        }

        return results;
    }

    public static IReadOnlyList<TorrentSearchHit> ParseSeasonEpisodes(
        string html,
        string seriesTitle,
        string seriesSlug,
        string baseUrl,
        int maxEpisodes)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var baseNormalized = LostFilmMirrors.NormalizeBaseUrl(baseUrl);
        var hits = new List<TorrentSearchHit>();

        foreach (var btn in doc.DocumentNode.SelectNodes("//div[contains(@class,'haveseen-btn') and @data-code]")
                     ?? Enumerable.Empty<HtmlNode>())
        {
            var code = btn.GetAttributeValue("data-code", string.Empty).Trim();
            var match = CodeRegex.Match(code);
            if (!match.Success)
                continue;

            var season = int.Parse(match.Groups["s"].Value, CultureInfo.InvariantCulture);
            var episode = int.Parse(match.Groups["e"].Value, CultureInfo.InvariantCulture);
            if (episode >= 999)
                continue;

            var row = btn.Ancestors("tr").FirstOrDefault();
            var beta = row?.SelectSingleNode("./td[contains(@class,'beta')]")?.InnerText?.Trim();
            var gamma = row?.SelectSingleNode("./td[contains(@class,'gamma')]");
            var episodeName = gamma?.SelectSingleNode(".//span[contains(@class,'small-text')]")?.InnerText?.Trim()
                              ?? gamma?.InnerText?.Trim();

            var label = string.IsNullOrWhiteSpace(beta)
                ? $"S{season:00}E{episode:00}"
                : beta;
            if (!string.IsNullOrWhiteSpace(episodeName))
                label = $"{label} · {CollapseWs(episodeName)}";

            var detailPath = $"/series/{seriesSlug}/season_{season}/episode_{episode}/";
            hits.Add(new TorrentSearchHit(
                code,
                $"{seriesTitle} · {label}",
                null,
                new Uri(new Uri(baseNormalized), detailPath).ToString()));
        }

        return hits
            .OrderByDescending(h => ParseCode(h.Id).Season)
            .ThenByDescending(h => ParseCode(h.Id).Episode)
            .Take(Math.Max(1, maxEpisodes))
            .ToList();
    }

    public static string? ParseDownloadRedirectUrl(string html, string baseUrl)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var meta = doc.DocumentNode.SelectSingleNode("//meta[translate(@http-equiv,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='refresh']");
        var content = meta?.GetAttributeValue("content", string.Empty) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(content))
            return null;

        var match = MetaRefreshRegex.Match(content.Trim());
        if (!match.Success)
            return null;

        var raw = match.Groups["url"].Value.Trim().Trim('"', '\'');
        if (raw.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return raw;

        return new Uri(new Uri(LostFilmMirrors.NormalizeBaseUrl(baseUrl)), raw).ToString();
    }

    public static IReadOnlyList<LostFilmQualityLink> ParseQualityLinks(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var links = new List<LostFilmQualityLink>();

        foreach (var item in doc.DocumentNode.SelectNodes("//div[contains(@class,'inner-box--item')]")
                     ?? Enumerable.Empty<HtmlNode>())
        {
            var label = item.SelectSingleNode(".//div[contains(@class,'inner-box--label')]")?.InnerText?.Trim();
            var href = item.SelectSingleNode(".//div[contains(@class,'inner-box--link')]//a")
                ?.GetAttributeValue("href", string.Empty)
                ?.Trim();
            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(href))
                continue;

            links.Add(new LostFilmQualityLink(NormalizeQuality(label), href));
        }

        return links;
    }

    public static string? SelectQualityUrl(IReadOnlyList<LostFilmQualityLink> links, string preferredQuality)
    {
        if (links.Count == 0)
            return null;

        var preferred = NormalizeQuality(preferredQuality);
        var exact = links.FirstOrDefault(l =>
            string.Equals(l.Quality, preferred, StringComparison.OrdinalIgnoreCase));
        if (exact is not null)
            return exact.Url;

        foreach (var fallback in new[] { "1080", "HD", "MP4", "SD" })
        {
            var hit = links.FirstOrDefault(l =>
                string.Equals(l.Quality, fallback, StringComparison.OrdinalIgnoreCase));
            if (hit is not null)
                return hit.Url;
        }

        return links[0].Url;
    }

    public static (int SeriesId, int Season, int Episode) ParseCode(string code)
    {
        var match = CodeRegex.Match(code.Trim());
        if (!match.Success)
            throw new ArgumentException($"Invalid LostFilm hit id '{code}'.", nameof(code));

        return (
            int.Parse(match.Groups["c"].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups["s"].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups["e"].Value, CultureInfo.InvariantCulture));
    }

    public static string NormalizeQuality(string quality)
    {
        var q = quality.Trim().ToUpperInvariant();
        return q switch
        {
            "1080P" or "1080" or "FULLHD" => "1080",
            "720" or "720P" or "HD" => "HD",
            "MP4" => "MP4",
            "SD" or "SD480" or "480" => "SD",
            _ => quality.Trim(),
        };
    }

    private static string ExtractSeriesSlug(string href)
    {
        var path = href.Trim();
        var idx = path.IndexOf("/series/", StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return string.Empty;

        var rest = path[(idx + "/series/".Length)..].Trim('/');
        var slash = rest.IndexOf('/');
        return slash < 0 ? rest : rest[..slash];
    }

    private static string CollapseWs(string text) =>
        string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}

internal sealed record LostFilmSeriesHit(string Slug, string Title, string Path);

internal sealed record LostFilmQualityLink(string Quality, string Url);
