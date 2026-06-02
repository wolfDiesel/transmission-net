using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Settings;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Handlers;

public sealed class TestConnectionHandler(ITransmissionClient transmissionClient)
{
    public async Task HandleAsync(
        DaemonConnectionDto connectionDto,
        AppSettings? existingSettings = null,
        CancellationToken cancellationToken = default)
    {
        var connection = SettingsMapper.ToConnection(connectionDto, existingSettings);
        SettingsValidator.ValidateConnection(connection);
        await transmissionClient.GetSessionAsync(connection, cancellationToken);
    }
}
