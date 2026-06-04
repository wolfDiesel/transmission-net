namespace TransmissonNET.Domain;

public static class UiLanguages
{
    public const string English = "en";
    public const string Russian = "ru";

    public const string Default = English;

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        English,
        Russian,
    };

    public static string Normalize(string? value) =>
        !string.IsNullOrWhiteSpace(value) && All.Contains(value)
            ? value
            : Default;
}
