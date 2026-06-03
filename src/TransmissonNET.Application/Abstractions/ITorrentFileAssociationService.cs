namespace TransmissonNET.Application.Abstractions;

public interface ITorrentFileAssociationService
{
    bool IsSupported { get; }

    bool IsDefaultHandler();

    bool HasDesktopEntry();

    Task RegisterAsync(CancellationToken cancellationToken = default);
}
