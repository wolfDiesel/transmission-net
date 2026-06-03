namespace TransmissonNET.Application.Contracts;

public sealed record TorrentMetainfoFromPathDto(
    string MetainfoBase64,
    TorrentMetainfoPreviewDto Preview);
