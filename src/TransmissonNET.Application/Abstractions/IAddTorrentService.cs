using TransmissonNET.Application.Contracts;

namespace TransmissonNET.Application.Abstractions;

public interface IAddTorrentService
{
    Task<TorrentAddResultDto> AddAsync(
        TorrentAddRequestDto request,
        CancellationToken cancellationToken = default);
}
