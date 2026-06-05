using Microsoft.Extensions.DependencyInjection;

namespace TransmissonNET.App.Avalonia.Services;

internal sealed class HandlerInvoker
{
    public async Task<T> InvokeAsync<T>(Func<IServiceProvider, Task<T>> action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var scope = AppServices.CreateScope();
        return await action(scope.ServiceProvider);
    }

    public async Task InvokeAsync(Func<IServiceProvider, Task> action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var scope = AppServices.CreateScope();
        await action(scope.ServiceProvider);
    }
}
