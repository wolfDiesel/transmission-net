using Avalonia.Controls;
using TransmissonNET.App.Avalonia.Services;

namespace TransmissonNET.App.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ToastHost.DataContext = AppServices.GetRequired<AppToastService>();
    }
}
