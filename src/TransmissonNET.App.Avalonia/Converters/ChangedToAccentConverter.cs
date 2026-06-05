using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TransmissonNET.App.Avalonia.Converters;

internal sealed class ChangedToAccentConverter : IValueConverter
{
    public static readonly ChangedToAccentConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var changed = value is true;
        var key = changed ? "AccentPrimaryBrush" : "ForegroundBrush";
        var app = global::Avalonia.Application.Current;
        if (app?.Resources.TryGetValue(key, out var resource) == true && resource is IBrush brush)
            return brush;
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
