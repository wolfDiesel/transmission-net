using TransmissonNET.Application.Contracts;
using TransmissonNET.Domain;

namespace TransmissonNET.Application.Tests;

internal static class TestTorrentTableSettingsDto
{
    public static TorrentTableSettingsDto Default
    {
        get
        {
            var table = TorrentTableSettings.CreateDefault();
            return new TorrentTableSettingsDto(
                table.Columns.Select(c => new TorrentTableColumnSettingDto(c.Id, c.Visible)).ToList(),
                table.SortColumnId,
                table.SortDescending);
        }
    }
}
