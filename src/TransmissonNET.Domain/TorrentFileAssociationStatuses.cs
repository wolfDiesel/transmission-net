namespace TransmissonNET.Domain;

public static class TorrentFileAssociationStatuses
{
    public const string NotAsked = "not_asked";
    public const string Registered = "registered";
    public const string Declined = "declined";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        NotAsked,
        Registered,
        Declined,
    };

    public static string Normalize(string? value) =>
        !string.IsNullOrWhiteSpace(value) && All.Contains(value) ? value : NotAsked;
}
