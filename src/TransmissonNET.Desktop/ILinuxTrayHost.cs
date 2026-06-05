namespace TransmissonNET.Desktop;

internal interface ILinuxTrayHost : IDisposable
{
    bool IsActive { get; }

    event Action? ShowRequested;
    event Action? QuitRequested;

    Task StartAsync(CancellationToken cancellationToken = default);
}
