using System.Globalization;
using Avalonia.Data.Converters;

namespace TransmissonNET.App.Avalonia.Converters;

internal sealed class PercentFormatterConverter : IValueConverter
{
    public static PercentFormatterConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            double ratio => $"{Math.Clamp(ratio, 0, 1) * 100:0.0}%",
            float ratio => $"{Math.Clamp(ratio, 0, 1) * 100:0.0}%",
            _ => "0.0%",
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
