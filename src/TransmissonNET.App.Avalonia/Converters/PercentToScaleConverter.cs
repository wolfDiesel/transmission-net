using System.Globalization;
using Avalonia.Data.Converters;

namespace TransmissonNET.App.Avalonia.Converters;

internal sealed class PercentToScaleConverter : IValueConverter
{
    public static PercentToScaleConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            double ratio => Math.Clamp(ratio, 0, 1),
            float ratio => Math.Clamp(ratio, 0, 1),
            _ => 0d,
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
