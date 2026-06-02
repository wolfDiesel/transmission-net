namespace TransmissonNET.Domain;

public sealed class TorrentFile
{
    public int Index { get; init; }
    public string Name { get; init; } = string.Empty;
    public long Length { get; init; }
    public long BytesCompleted { get; init; }
    public bool Wanted { get; init; }
    public int Priority { get; init; }
}
