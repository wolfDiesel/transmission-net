namespace TransmissonNET.Application.Contracts;

public sealed record TorrentRenameBatchRequestDto(IReadOnlyList<TorrentRenameOperationDto> Operations);
