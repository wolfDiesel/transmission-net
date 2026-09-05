using System.Reflection;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.App.Avalonia.Views;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;
    private readonly LocalizationService _localization;
    private readonly TorrentsViewModel _torrents;
    private readonly SearchViewModel _search;
    private readonly AddTorrentViewModel _addTorrent;
    private readonly SettingsViewModel _settings;
    private readonly StatusBarViewModel _statusBar;
    private readonly DispatcherTimer _statusTimer = new() { Interval = TimeSpan.FromSeconds(3) };

    public string AppVersion => AppVersionInfo.Version;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private string _title = "TransmissionNET";

    [ObservableProperty] private bool _navTorrentsActive = true;
    [ObservableProperty] private bool _navSearchActive;
    [ObservableProperty] private bool _navAddActive;
    [ObservableProperty] private bool _navSettingsActive;

    public StatusBarViewModel StatusBar => _statusBar;

    public MainWindowViewModel(
        NavigationService navigation,
        LocalizationService localization,
        TorrentsViewModel torrents,
        SearchViewModel search,
        AddTorrentViewModel addTorrent,
        SettingsViewModel settings,
        StatusBarViewModel statusBar)
    {
        _navigation = navigation;
        _localization = localization;
        _torrents = torrents;
        _search = search;
        _addTorrent = addTorrent;
        _settings = settings;
        _statusBar = statusBar;
        _currentPage = torrents;

        _navigation.CurrentPageChanged += OnNavigationChanged;
        _localization.LanguageChanged += RefreshNavLabels;
        _statusTimer.Tick += async (_, _) =>
        {
            if (!NavTorrentsActive)
                await _statusBar.RefreshAsync();
        };
        _statusTimer.Start();
    }

    public async Task InitializeAsync()
    {
        await _torrents.InitializeAsync();
        await _addTorrent.InitializeAsync();
        await _settings.InitializeAsync();
        if (!NavTorrentsActive)
            await _statusBar.RefreshAsync();
        RefreshNavLabels();
    }

    [RelayCommand]
    private void NavigateTorrents() => _navigation.Navigate(AppPage.Torrents);

    [RelayCommand]
    private void NavigateSearch() => _navigation.Navigate(AppPage.Search);

    [RelayCommand]
    private void NavigateAddTorrent() => _navigation.Navigate(AppPage.AddTorrent);

    [RelayCommand]
    private void NavigateSettings() => _navigation.Navigate(AppPage.Settings);

    [RelayCommand]
    private void ShowAbout() => AboutWindow.Show(GetOwnerWindow(), _localization);

    private static global::Avalonia.Controls.Window? GetOwnerWindow()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime
            is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }

    private void OnNavigationChanged()
    {
        CurrentPage = _navigation.CurrentPage switch
        {
            AppPage.Search => _search,
            AppPage.AddTorrent => _addTorrent,
            AppPage.Settings => _settings,
            _ => _torrents,
        };

        NavTorrentsActive = _navigation.CurrentPage == AppPage.Torrents;
        NavSearchActive = _navigation.CurrentPage == AppPage.Search;
        NavAddActive = _navigation.CurrentPage == AppPage.AddTorrent;
        NavSettingsActive = _navigation.CurrentPage == AppPage.Settings;

        if (NavSearchActive)
            _search.ReloadProviders(notifyLoadErrors: true);

        if (!NavTorrentsActive)
            _ = _statusBar.RefreshAsync();
    }

    private void RefreshNavLabels()
    {
        Title = "TransmissionNET";
        OnPropertyChanged(nameof(NavTorrentsLabel));
        OnPropertyChanged(nameof(NavSearchLabel));
        OnPropertyChanged(nameof(NavAddLabel));
        OnPropertyChanged(nameof(NavSettingsLabel));
        OnPropertyChanged(nameof(NavAboutLabel));
    }

    public string NavTorrentsLabel => _localization.T("nav.torrents");
    public string NavSearchLabel => _localization.T("nav.search");
    public string NavAddLabel => _localization.T("nav.addTorrent");
    public string NavSettingsLabel => _localization.T("nav.settings");
    public string NavAboutLabel => _localization.T("nav.about");
}

internal static class AppVersionInfo
{
    public static string Version =>
        typeof(AppVersionInfo).Assembly
            .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? "0.0.0-dev";
}
