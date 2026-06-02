namespace TransmissonNET.Application.Contracts;

public sealed record TorrentActionDto(
    string Action,
    IReadOnlyList<int> Ids,
    bool DeleteLocalData = false,
    string? Priority = null,
    string? Location = null,
    bool Move = false,
    string? Path = null,
    string? Name = null);
