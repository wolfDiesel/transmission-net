using System.Globalization;
using Avalonia.Data.Converters;
using TransmissonNET.App.Avalonia.Services;

namespace TransmissonNET.App.Avalonia.Converters;

internal sealed class SpeedFormatterConverter : IValueConverter
{
    public static SpeedFormatterConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            long bytes => DisplayFormatter.Speed(bytes),
            int bytes => DisplayFormatter.Speed(bytes),
            double bytes => DisplayFormatter.Speed((long)bytes),
            _ => DisplayFormatter.Speed(0),
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
