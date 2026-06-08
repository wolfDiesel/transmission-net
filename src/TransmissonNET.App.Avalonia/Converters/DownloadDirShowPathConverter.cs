using System.Globalization;
using Avalonia.Data.Converters;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.App.Avalonia.Converters;

public sealed class DownloadDirShowPathConverter : IValueConverter
{
    public static readonly DownloadDirShowPathConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string path
        && !string.Equals(DownloadDirHistoryHelper.FolderDisplayName(path), path, StringComparison.Ordinal);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
