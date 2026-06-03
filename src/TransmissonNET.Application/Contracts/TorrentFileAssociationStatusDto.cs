namespace TransmissonNET.Application.Contracts;

public sealed record TorrentFileAssociationStatusDto(
    bool IsSupported,
    bool HasDesktopEntry,
    bool IsDefaultHandler,
    string PromptStatus,
    bool ShouldPrompt);
