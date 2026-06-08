using System.Collections;
using Avalonia;
using Avalonia.Controls;
using TransmissonNET.Application.Settings;

namespace TransmissonNET.App.Avalonia.Views;

public partial class DownloadDirPicker : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<DownloadDirPicker, string?>(nameof(Text), string.Empty, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<DownloadDirPicker, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<DownloadDirPicker, string?>(nameof(PlaceholderText));

    public DownloadDirPicker()
    {
        InitializeComponent();
        Input.ItemFilter = static (search, item) =>
            item is string path && DownloadDirHistoryHelper.MatchesQuery(path, search ?? string.Empty);
        Input.LostFocus += (_, _) => Input.IsDropDownOpen = false;
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
            Input.ItemsSource = change.NewValue as IEnumerable;
    }
}
