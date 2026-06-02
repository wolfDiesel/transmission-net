namespace TransmissonNET.Application.Contracts;

public sealed record TorrentTableColumnSettingDto(string Id, bool Visible, int? WidthPx = null);

public sealed record TorrentTableSettingsDto(
    IReadOnlyList<TorrentTableColumnSettingDto> Columns,
    string SortColumnId,
    bool SortDescending);
