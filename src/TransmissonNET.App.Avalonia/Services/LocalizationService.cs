using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using TransmissonNET.Domain;

namespace TransmissonNET.App.Avalonia.Services;

internal sealed class LocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _catalogs = new(StringComparer.Ordinal);
    private string _language = UiLanguages.Default;

    public event Action? LanguageChanged;

    public string Language => _language;

    public LocalizationService()
    {
        LoadCatalog("en");
        LoadCatalog("ru");
        LoadCatalog("de");
        LoadCatalog("fr");
    }

    public void SetLanguage(string? language)
    {
        var normalized = UiLanguages.Normalize(language);
        if (_language == normalized)
            return;

        _language = normalized;
        LanguageChanged?.Invoke();
    }

    public string T(string key) =>
        _catalogs.TryGetValue(_language, out var catalog) && catalog.TryGetValue(key, out var value)
            ? value
            : _catalogs["en"].TryGetValue(key, out var fallback)
                ? fallback
                : key;

    public string T(string key, IReadOnlyDictionary<string, string> args)
    {
        var text = T(key);
        foreach (var (name, value) in args)
            text = text.Replace($"{{{name}}}", value, StringComparison.Ordinal);
        return text;
    }

    public IReadOnlyList<string> TList(string key)
    {
        var text = T(key);
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();
        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private void LoadCatalog(string language)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith($"Localization.{language}.json", StringComparison.Ordinal));
        if (resourceName is null)
            return;

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing locale resource {language}");
        var json = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
            ?? new Dictionary<string, string>();
        _catalogs[language] = json;
    }
}

internal static class LocalizationExtensions
{
    private static readonly Regex TokenRegex = new(@"\{(\w+)\}", RegexOptions.Compiled);

    public static string Format(this LocalizationService localization, string key, params (string Name, string Value)[] args)
    {
        var text = localization.T(key);
        foreach (var (name, value) in args)
            text = text.Replace($"{{{name}}}", value, StringComparison.Ordinal);
        return text;
    }
}
