namespace TransmissonNET.App.Avalonia.Services;

internal enum AppPage
{
    Torrents,
    AddTorrent,
    Settings,
}

internal sealed class NavigationService
{
    public event Action? CurrentPageChanged;

    public AppPage CurrentPage { get; private set; } = AppPage.Torrents;

    public void Navigate(AppPage page)
    {
        if (CurrentPage == page)
            return;

        CurrentPage = page;
        CurrentPageChanged?.Invoke();
    }
}
