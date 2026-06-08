using System.Globalization;
using Avalonia.Data.Converters;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.App.Avalonia.Converters;

public sealed class DownloadDirDisplayNameConverter : IValueConverter
{
    public static readonly DownloadDirDisplayNameConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string path ? DownloadDirHistoryHelper.FolderDisplayName(path) : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
