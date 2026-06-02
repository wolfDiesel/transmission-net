using TransmissonNET.Application.Contracts;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Settings;

internal static class TorrentTableSettingsMapper
{
    public static TorrentTableSettingsDto ToDto(TorrentTableSettings settings) =>
        new(
            settings.Columns
                .Select(c => new TorrentTableColumnSettingDto(c.Id, c.Visible, c.WidthPx))
                .ToList(),
            settings.SortColumnId,
            settings.SortDescending);

    public static TorrentTableSettings ToDomain(TorrentTableSettingsDto? dto, TorrentTableSettings? existing = null)
    {
        if (dto is null || dto.Columns.Count == 0)
            return existing ?? TorrentTableSettings.CreateDefault();

        var columns = dto.Columns
            .Where(c => TorrentTableColumnIds.All.Contains(c.Id))
            .Select(c => new TorrentTableColumnSetting(
                c.Id,
                c.Visible,
                ResolveWidth(c.WidthPx, existing, c.Id)))
            .ToList();

        if (columns.Count == 0)
            return existing ?? TorrentTableSettings.CreateDefault();

        var knownIds = columns.Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var fallback in existing?.Columns ?? TorrentTableSettings.CreateDefault().Columns)
        {
            if (!knownIds.Contains(fallback.Id))
                columns.Add(fallback);
        }

        var sortColumnId = TorrentTableColumnIds.All.Contains(dto.SortColumnId)
            ? dto.SortColumnId
            : existing?.SortColumnId ?? TorrentTableColumnIds.Name;

        return new TorrentTableSettings(columns, sortColumnId, dto.SortDescending);
    }

    private static int? ResolveWidth(int? dtoWidth, TorrentTableSettings? existing, string columnId)
    {
        if (dtoWidth is not null)
            return Math.Clamp(dtoWidth.Value, 48, 640);

        return existing?.Columns.FirstOrDefault(c => c.Id == columnId)?.WidthPx;
    }
}
