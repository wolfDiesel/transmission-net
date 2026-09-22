using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;

namespace TransmissonNET.App.Avalonia.Services;

internal sealed class AddTorrentService(HandlerInvoker handlers) : IAddTorrentService
{
    public Task<TorrentAddResultDto> AddAsync(
        TorrentAddRequestDto request,
        CancellationToken cancellationToken = default) =>
        handlers.InvokeAsync(
            sp => sp.GetRequiredService<AddTorrentHandler>().HandleAsync(request, cancellationToken),
            cancellationToken);
}
