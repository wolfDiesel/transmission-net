using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Exceptions;
using TransmissonNET.Application.Handlers;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.App.Api;

internal static class ApiEndpoints
{
    public static void MapTransmissonNetApi(this WebApplication app)
    {
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

        app.MapGet("/api/settings", async (GetSettingsHandler handler, CancellationToken ct) =>
        {
            var settings = await handler.HandleAsync(ct);
            return Results.Ok(settings);
        });

        app.MapPut("/api/settings", async (AppSettingsDto dto, SaveSettingsHandler handler, CancellationToken ct) =>
        {
            try
            {
                var saved = await handler.HandleAsync(dto, ct);
                return Results.Ok(saved);
            }
            catch (SettingsValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapPost("/api/connection/test", async (
            DaemonConnectionDto dto,
            TestConnectionHandler handler,
            ISettingsStore settingsStore,
            CancellationToken ct) =>
        {
            try
            {
                var existing = await settingsStore.LoadAsync(ct);
                await handler.HandleAsync(dto, existing, ct);
                return Results.Ok(new { status = "connected" });
            }
            catch (SettingsValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (DaemonConnectionException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
            }
        });

        app.MapGet("/api/status", async (bool? counts, GetDaemonStatusHandler handler, CancellationToken ct) =>
        {
            var includeCounts = counts is not false;
            var status = await handler.HandleAsync(includeCounts, ct);
            return Results.Ok(status);
        });

        app.MapGet("/api/daemon/session-settings", async (
            GetDaemonSessionSettingsHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                var settings = await handler.HandleAsync(ct);
                return Results.Ok(settings);
            }
            catch (DaemonConnectionException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
            }
        });

        app.MapPut("/api/daemon/session-settings", async (
            DaemonSessionSettingsDto dto,
            SaveDaemonSessionSettingsHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                var saved = await handler.HandleAsync(dto, ct);
                return Results.Ok(saved);
            }
            catch (SettingsValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (DaemonConnectionException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
            }
        });

        app.MapGet("/api/torrents", async (GetTorrentsHandler handler, CancellationToken ct) =>
        {
            try
            {
                var torrents = await handler.HandleAsync(ct);
                return Results.Ok(TorrentDtoMapper.ToDtoList(torrents));
            }
            catch (DaemonConnectionException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
            }
        });

        app.MapGet("/api/torrents/{id:int}", async (int id, GetTorrentDetailsHandler handler, CancellationToken ct) =>
        {
            try
            {
                var details = await handler.HandleAsync(id, ct);
                if (details is null)
                    return Results.NotFound();

                return Results.Ok(TorrentDetailsDtoMapper.ToDto(details));
            }
            catch (DaemonConnectionException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
            }
        });

        app.MapPost("/api/torrents/actions", async (
            TorrentActionDto dto,
            ExecuteTorrentActionHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                await handler.HandleAsync(dto, ct);
                return Results.Ok(new { status = "ok" });
            }
            catch (SettingsValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (DaemonConnectionException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
            }
        });

        app.MapPost("/api/torrents/inspect", async (
            TorrentMetainfoInspectRequestDto dto,
            InspectTorrentMetainfoHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                var preview = await handler.HandleAsync(dto, ct);
                return Results.Ok(preview);
            }
            catch (SettingsValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapPost("/api/torrents/inspect-path", async (
            TorrentMetainfoInspectPathRequestDto dto,
            InspectTorrentMetainfoFromPathHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                var preview = await handler.HandleAsync(dto, ct);
                return Results.Ok(preview);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        });

        app.MapGet("/api/desktop/torrent-association", async (
            GetTorrentFileAssociationStatusHandler handler,
            CancellationToken ct) =>
        {
            var status = await handler.HandleAsync(ct);
            return Results.Ok(status);
        });

        app.MapPost("/api/desktop/torrent-association/register", async (
            RegisterTorrentFileAssociationHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                await handler.HandleAsync(ct);
                return Results.Ok(new { status = "registered" });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (PlatformNotSupportedException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapPost("/api/desktop/torrent-association/decline", async (
            DeclineTorrentFileAssociationHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(ct);
            return Results.Ok(new { status = "declined" });
        });

        app.MapGet("/api/desktop/pending-torrent-path", async (
            GetPendingTorrentLaunchPathHandler handler,
            CancellationToken ct) =>
        {
            var path = await handler.HandleAsync(ct);
            return Results.Ok(new { path });
        });

        app.MapPost("/api/torrents/add", async (
            TorrentAddRequestDto dto,
            AddTorrentHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(dto, ct);
                return Results.Ok(result);
            }
            catch (SettingsValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (DaemonConnectionException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
            }
        });

        app.MapPost("/api/torrents/{id:int}/rename-batch", async (
            int id,
            TorrentRenameBatchRequestDto dto,
            ExecuteTorrentRenameBatchHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(id, dto, ct);
                return Results.Ok(result);
            }
            catch (SettingsValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (DaemonConnectionException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
            }
        });
    }
}
