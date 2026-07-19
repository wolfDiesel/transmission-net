namespace TransmissonNET.Providers.Abstractions;

public sealed record TorrentSearchHit(
    string Id,
    string Title,
    long? SizeBytes,
    string? DetailUrl);
