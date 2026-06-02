namespace TransmissonNET.Application.Contracts;

public sealed record TorrentRenameFailureDto(string Path, string Error);

public sealed record TorrentRenameBatchResultDto(
    int Applied,
    IReadOnlyList<TorrentRenameFailureDto> Failures);
