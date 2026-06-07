using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TransmissonNET.App.Avalonia.Desktop;

namespace TransmissonNET.App.Avalonia.Views;

public partial class RemoveTorrentDialog : Window
{
    private const double MaxHeightScreenFraction = 2.0 / 3.0;

    public bool Confirmed { get; private set; }
    public bool DeleteLocalData => DeleteDataCheck.IsChecked == true;

    public RemoveTorrentDialog(string promptText)
    {
        InitializeComponent();
        TorrentNameText.Text = promptText;
        WindowEscClose.Attach(this);
        Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Opened -= OnOpened;
        ApplyHeightLimits();
    }

    private void ApplyHeightLimits()
    {
        var maxDialogHeight = GetMaxDialogHeight();
        MaxHeight = maxDialogHeight;

        var chrome = Padding.Top + Padding.Bottom + 32;
        var contentWidth = Math.Max(200, Bounds.Width - chrome);
        FooterPanel.Measure(new Size(contentWidth, double.PositiveInfinity));
        var footerHeight = FooterPanel.DesiredSize.Height + 12;

        PromptScroll.MaxHeight = Math.Max(80, maxDialogHeight - footerHeight - chrome);
    }

    private double GetMaxDialogHeight()
    {
        var screen = Screens?.ScreenFromWindow(this) ?? Screens?.Primary;
        var workingHeight = screen?.WorkingArea.Height ?? 900;
        return workingHeight * MaxHeightScreenFraction;
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close();

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        Confirmed = true;
        Close();
    }
}
