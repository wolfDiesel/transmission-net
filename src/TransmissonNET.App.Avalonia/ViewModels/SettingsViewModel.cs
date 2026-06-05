using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.App.Avalonia.Services;
using TransmissonNET.App.Avalonia.Theme;
using TransmissonNET.Domain;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly HandlerInvoker _handlers;
    private readonly LocalizationService _localization;
    private readonly ThemeService _theme;
    private readonly AppToastService _toasts;
    private AppSettingsDto? _loaded;

    [ObservableProperty] private string _host = "localhost";
    [ObservableProperty] private int _port = 9091;
    [ObservableProperty] private string _rpcPath = "/transmission/rpc";
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private int _refreshIntervalSeconds = 3;
    [ObservableProperty] private int _windowWidth = 1200;
    [ObservableProperty] private int _windowHeight = 800;
    [ObservableProperty] private string _colorScheme = UiColorSchemes.Default;
    [ObservableProperty] private string _appearance = UiAppearances.Default;
    [ObservableProperty] private string _language = UiLanguages.Default;
    [ObservableProperty] private bool _trayEnabled = true;
    [ObservableProperty] private bool _minimizeToTray;
    [ObservableProperty] private bool _closeToTray = true;
    [ObservableProperty] private string _daemonDownloadDir = string.Empty;
    [ObservableProperty] private string _daemonIncompleteDir = string.Empty;
    [ObservableProperty] private bool _daemonIncompleteDirEnabled;
    [ObservableProperty] private bool _daemonTrashTorrent;
    [ObservableProperty] private int _daemonPeerLimitGlobal = 200;
    [ObservableProperty] private int _daemonPeerLimitPerTorrent = 50;
    [ObservableProperty] private int _daemonDownloadLimit;
    [ObservableProperty] private int _daemonUploadLimit;
    [ObservableProperty] private bool _daemonDownloadLimitEnabled;
    [ObservableProperty] private bool _daemonUploadLimitEnabled;
    [ObservableProperty] private double _daemonSeedRatioLimit = 2;
    [ObservableProperty] private bool _daemonSeedRatioLimited;
    [ObservableProperty] private int _daemonIdleSeedingLimitMinutes = 30;
    [ObservableProperty] private bool _daemonIdleSeedingLimitEnabled;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _traySupported = OperatingSystem.IsLinux();

    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _pageSubtitle = string.Empty;
    [ObservableProperty] private string _tabConnection = string.Empty;
    [ObservableProperty] private string _tabDaemon = string.Empty;
    [ObservableProperty] private string _tabInterface = string.Empty;

    public ObservableCollection<PaletteChipViewModel> PaletteChips { get; } = new();

    public SettingsViewModel(
        HandlerInvoker handlers,
        LocalizationService localization,
        ThemeService theme,
        AppToastService toasts)
    {
        _handlers = handlers;
        _localization = localization;
        _theme = theme;
        _toasts = toasts;
        _localization.LanguageChanged += RefreshLocalizedUi;
        RebuildPaletteChips();
        RefreshLocalizedUi();
    }

    public bool IsAppearanceLight => Appearance == UiAppearances.Light;
    public bool IsAppearanceDark => Appearance == UiAppearances.Dark;
    public bool IsAppearanceSystem => Appearance == UiAppearances.System;
    public bool IsLanguageEnglish => Language == UiLanguages.English;
    public bool IsLanguageRussian => Language == UiLanguages.Russian;
    public bool IsLanguageGerman => Language == UiLanguages.German;
    public bool IsLanguageFrench => Language == UiLanguages.French;

    public string ConnectionTitle => T("settings.connection.title");
    public string ConnectionSubtitle => T("settings.connection.subtitle");
    public string SectionConnection => T("settings.connection.sectionConnection");
    public string SectionAuth => T("settings.connection.sectionAuth");
    public string HostLabel => T("settings.connection.host");
    public string PortLabel => T("settings.connection.port");
    public string RpcPathLabel => T("settings.connection.rpcPath");
    public string UsernameLabel => T("settings.connection.username");
    public string PasswordLabel => T("settings.connection.password");
    public string PasswordPlaceholder => T("settings.connection.passwordPlaceholder");
    public string PasswordHint => T("settings.connection.passwordHint");
    public string TestConnectionLabel => T("settings.connection.test");
    public string SaveSettingsLabel => T("common.saveSettings");

    public string DaemonPreferencesTitle => T("settings.daemon.preferencesTitle");
    public string DaemonPreferencesSubtitle => T("settings.daemon.preferencesSubtitle");
    public string DaemonStorageTitle => T("settings.daemon.storage");
    public string DownloadDirLabel => T("settings.daemon.downloadDir");
    public string IncompleteDirLabel => T("settings.daemon.incompleteDir");
    public string IncompleteDirEnabledLabel => T("settings.daemon.incompleteDirEnabled");
    public string TrashTorrentLabel => T("settings.daemon.trashTorrent");
    public string PeersTitle => T("settings.daemon.peers");
    public string GlobalPeerLimitLabel => T("settings.daemon.globalPeerLimit");
    public string PeerLimitPerTorrentLabel => T("settings.daemon.peerLimitPerTorrent");
    public string SpeedLimitsTitle => T("settings.daemon.speedLimits");
    public string EnableDownloadLimitLabel => T("settings.daemon.enableDownloadLimit");
    public string DownloadLimitLabel => T("settings.daemon.downloadLimit");
    public string EnableUploadLimitLabel => T("settings.daemon.enableUploadLimit");
    public string UploadLimitLabel => T("settings.daemon.uploadLimit");
    public string ReloadDaemonLabel => T("settings.daemon.reload");
    public string ApplyDaemonLabel => T("settings.daemon.apply");

    public string AppearanceTitle => T("settings.appearance.title");
    public string ThemeSectionLabel => T("settings.appearance.theme");
    public string AppearanceLightLabel => T("settings.appearance.light");
    public string AppearanceDarkLabel => T("settings.appearance.dark");
    public string AppearanceSystemLabel => T("settings.appearance.system");
    public string ColorSchemeSectionLabel => T("settings.appearance.colorScheme");
    public string LanguageTitle => T("settings.language.title");
    public string LanguageLabel => T("settings.language.label");
    public string LanguageEnglishLabel => T("settings.language.en");
    public string LanguageRussianLabel => T("settings.language.ru");
    public string LanguageGermanLabel => T("settings.language.de");
    public string LanguageFrenchLabel => T("settings.language.fr");
    public string WindowTitle => T("settings.window.title");
    public string RefreshIntervalLabel => T("settings.window.refreshInterval");
    public string WindowWidthLabel => T("settings.window.windowWidth");
    public string WindowHeightLabel => T("settings.window.windowHeight");
    public string WindowHint => T("settings.window.hint");
    public string TrayTitle => T("settings.tray.title");
    public string TrayEnabledLabel => T("settings.tray.enabled");
    public string TrayCloseLabel => T("settings.tray.closeToTray");
    public string TrayMinimizeLabel => T("settings.tray.minimizeToTray");
    public string TorrentAssociationTitle => T("settings.torrentAssociation.title");
    public string RegisterAssociationLabel => T("settings.torrentAssociation.register");

    public async Task InitializeAsync()
    {
        var settings = await _handlers.InvokeAsync(sp => sp.GetRequiredService<GetSettingsHandler>().HandleAsync());
        ApplyAppSettings(settings);
        await LoadDaemonSessionAsync();
    }

    [RelayCommand]
    private void SelectAppearance(string value)
    {
        if (UiAppearances.All.Contains(value))
            Appearance = value;
    }

    [RelayCommand]
    private void SelectColorScheme(string value)
    {
        if (UiColorSchemes.All.Contains(value))
            ColorScheme = value;
    }

    [RelayCommand]
    private void SelectLanguage(string value)
    {
        if (UiLanguages.All.Contains(value))
            Language = value;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            var saved = await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<SaveSettingsHandler>().HandleAsync(BuildSettingsDto()));
            ApplyAppSettings(saved);
            _toasts.ShowSuccess(T("settings.saved"));
        }
        catch (Exception ex)
        {
            _toasts.ShowError(T("settings.saveFailed"), ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        IsBusy = true;
        try
        {
            var dto = new DaemonConnectionDto(Host, Port, RpcPath, Username, string.IsNullOrEmpty(Password) ? null : Password);
            await _handlers.InvokeAsync(async sp =>
            {
                var existing = await sp.GetRequiredService<ISettingsStore>().LoadAsync();
                await sp.GetRequiredService<TestConnectionHandler>().HandleAsync(dto, existing);
            });
            _toasts.ShowSuccess(T("settings.connection.connectionOk"));
        }
        catch (Exception ex)
        {
            _toasts.ShowError(T("settings.connection.connectionFailed"), ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadDaemonSessionAsync()
    {
        try
        {
            var daemon = await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<GetDaemonSessionSettingsHandler>().HandleAsync());
            ApplyDaemonSession(daemon);
        }
        catch (Exception ex)
        {
            _toasts.ShowError(T("settings.daemon.loadFailed"), ex.Message);
        }
    }

    [RelayCommand]
    private async Task SaveDaemonSessionAsync()
    {
        IsBusy = true;
        try
        {
            await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<SaveDaemonSessionSettingsHandler>().HandleAsync(BuildDaemonSessionDto()));
            _toasts.ShowSuccess(T("settings.daemon.applied"));
        }
        catch (Exception ex)
        {
            _toasts.ShowError(T("settings.daemon.saveFailed"), ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegisterTorrentAssociationAsync()
    {
        try
        {
            await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<RegisterTorrentFileAssociationHandler>().HandleAsync());
            _toasts.ShowSuccess(T("settings.torrentAssociation.success"));
        }
        catch (Exception ex)
        {
            _toasts.ShowError(T("settings.torrentAssociation.failed"), ex.Message);
        }
    }

    partial void OnColorSchemeChanged(string value)
    {
        UpdatePaletteSelection();
        ApplyThemePreview();
    }

    partial void OnAppearanceChanged(string value)
    {
        NotifyAppearanceFlags();
        ApplyThemePreview();
    }

    partial void OnLanguageChanged(string value)
    {
        _localization.SetLanguage(value);
        RefreshLocalizedUi();
        NotifyLanguageFlags();
    }

    private AppSettingsDto BuildSettingsDto()
    {
        var table = _loaded?.Ui.TorrentTable
            ?? new TorrentTableSettingsDto([], "name", false);
        return new AppSettingsDto(
            new DaemonConnectionDto(Host, Port, RpcPath, Username, string.IsNullOrEmpty(Password) ? null : Password),
            new UiSettingsDto(
                RefreshIntervalSeconds,
                WindowWidth,
                WindowHeight,
                table,
                ColorScheme,
                Appearance,
                _loaded?.Ui.DownloadDirHistory,
                _loaded?.Ui.TorrentFileAssociation ?? TorrentFileAssociationStatuses.NotAsked,
                TrayEnabled,
                MinimizeToTray,
                CloseToTray,
                Language));
    }

    private DaemonSessionSettingsDto BuildDaemonSessionDto() =>
        new(
            DaemonDownloadDir,
            DaemonIncompleteDir,
            DaemonIncompleteDirEnabled,
            DaemonTrashTorrent,
            DaemonPeerLimitGlobal,
            DaemonPeerLimitPerTorrent,
            DaemonDownloadLimit,
            DaemonUploadLimit,
            DaemonDownloadLimitEnabled,
            DaemonUploadLimitEnabled,
            DaemonSeedRatioLimit,
            DaemonSeedRatioLimited,
            DaemonIdleSeedingLimitMinutes,
            DaemonIdleSeedingLimitEnabled);

    private void ApplyAppSettings(AppSettingsDto settings)
    {
        _loaded = settings;
        Host = settings.Daemon.Host;
        Port = settings.Daemon.Port;
        RpcPath = settings.Daemon.RpcPath;
        Username = settings.Daemon.Username;
        Password = settings.Daemon.Password ?? string.Empty;
        RefreshIntervalSeconds = settings.Ui.RefreshIntervalSeconds;
        WindowWidth = settings.Ui.WindowWidth;
        WindowHeight = settings.Ui.WindowHeight;
        ColorScheme = settings.Ui.ColorScheme;
        Appearance = settings.Ui.Appearance;
        Language = settings.Ui.Language;
        TrayEnabled = settings.Ui.TrayEnabled;
        MinimizeToTray = settings.Ui.MinimizeToTray;
        CloseToTray = settings.Ui.CloseToTray;
        _localization.SetLanguage(settings.Ui.Language);
        _theme.Apply(settings.Ui);
        UpdatePaletteSelection();
        NotifyAppearanceFlags();
        RefreshLocalizedUi();
    }

    private void ApplyDaemonSession(DaemonSessionSettingsDto daemon)
    {
        DaemonDownloadDir = daemon.DownloadDir;
        DaemonIncompleteDir = daemon.IncompleteDir;
        DaemonIncompleteDirEnabled = daemon.IncompleteDirEnabled;
        DaemonTrashTorrent = daemon.TrashOriginalTorrentFiles;
        DaemonPeerLimitGlobal = daemon.PeerLimitGlobal;
        DaemonPeerLimitPerTorrent = daemon.PeerLimitPerTorrent;
        DaemonDownloadLimit = daemon.SpeedLimitDownKbps;
        DaemonUploadLimit = daemon.SpeedLimitUpKbps;
        DaemonDownloadLimitEnabled = daemon.SpeedLimitDownEnabled;
        DaemonUploadLimitEnabled = daemon.SpeedLimitUpEnabled;
        DaemonSeedRatioLimit = daemon.SeedRatioLimit;
        DaemonSeedRatioLimited = daemon.SeedRatioLimited;
        DaemonIdleSeedingLimitMinutes = daemon.IdleSeedingLimitMinutes;
        DaemonIdleSeedingLimitEnabled = daemon.IdleSeedingLimitEnabled;
    }

    private void ApplyThemePreview() =>
        _theme.Apply(BuildSettingsDto().Ui);

    private void RebuildPaletteChips()
    {
        PaletteChips.Clear();
        foreach (var palette in AccentPalettes.All)
        {
            PaletteChips.Add(new PaletteChipViewModel
            {
                Id = palette.Id,
                ColorHex = palette.Primary,
                Label = T($"settings.appearance.colors.{palette.Id}"),
                IsSelected = ColorScheme == palette.Id,
            });
        }
    }

    private void UpdatePaletteSelection()
    {
        foreach (var chip in PaletteChips)
            chip.IsSelected = chip.Id == ColorScheme;
    }

    private void NotifyAppearanceFlags()
    {
        OnPropertyChanged(nameof(IsAppearanceLight));
        OnPropertyChanged(nameof(IsAppearanceDark));
        OnPropertyChanged(nameof(IsAppearanceSystem));
    }

    private void RefreshLocalizedUi()
    {
        PageTitle = T("settings.title");
        PageSubtitle = T("settings.subtitle");
        TabConnection = T("settings.tabs.connection");
        TabDaemon = T("settings.tabs.daemon");
        TabInterface = T("settings.tabs.ui");

        RebuildPaletteChips();
        NotifyAppearanceFlags();
        NotifyLanguageFlags();
        NotifyLabelPropertiesChanged();
    }

    private void NotifyLanguageFlags()
    {
        OnPropertyChanged(nameof(IsLanguageEnglish));
        OnPropertyChanged(nameof(IsLanguageRussian));
        OnPropertyChanged(nameof(IsLanguageGerman));
        OnPropertyChanged(nameof(IsLanguageFrench));
        OnPropertyChanged(nameof(LanguageEnglishLabel));
        OnPropertyChanged(nameof(LanguageRussianLabel));
        OnPropertyChanged(nameof(LanguageGermanLabel));
        OnPropertyChanged(nameof(LanguageFrenchLabel));
    }

    private void NotifyLabelPropertiesChanged()
    {
        OnPropertyChanged(nameof(ConnectionTitle));
        OnPropertyChanged(nameof(ConnectionSubtitle));
        OnPropertyChanged(nameof(SectionConnection));
        OnPropertyChanged(nameof(SectionAuth));
        OnPropertyChanged(nameof(HostLabel));
        OnPropertyChanged(nameof(PortLabel));
        OnPropertyChanged(nameof(RpcPathLabel));
        OnPropertyChanged(nameof(UsernameLabel));
        OnPropertyChanged(nameof(PasswordLabel));
        OnPropertyChanged(nameof(PasswordPlaceholder));
        OnPropertyChanged(nameof(PasswordHint));
        OnPropertyChanged(nameof(TestConnectionLabel));
        OnPropertyChanged(nameof(SaveSettingsLabel));
        OnPropertyChanged(nameof(DaemonPreferencesTitle));
        OnPropertyChanged(nameof(DaemonPreferencesSubtitle));
        OnPropertyChanged(nameof(DaemonStorageTitle));
        OnPropertyChanged(nameof(DownloadDirLabel));
        OnPropertyChanged(nameof(IncompleteDirLabel));
        OnPropertyChanged(nameof(IncompleteDirEnabledLabel));
        OnPropertyChanged(nameof(TrashTorrentLabel));
        OnPropertyChanged(nameof(PeersTitle));
        OnPropertyChanged(nameof(GlobalPeerLimitLabel));
        OnPropertyChanged(nameof(PeerLimitPerTorrentLabel));
        OnPropertyChanged(nameof(SpeedLimitsTitle));
        OnPropertyChanged(nameof(EnableDownloadLimitLabel));
        OnPropertyChanged(nameof(DownloadLimitLabel));
        OnPropertyChanged(nameof(EnableUploadLimitLabel));
        OnPropertyChanged(nameof(UploadLimitLabel));
        OnPropertyChanged(nameof(ReloadDaemonLabel));
        OnPropertyChanged(nameof(ApplyDaemonLabel));
        OnPropertyChanged(nameof(AppearanceTitle));
        OnPropertyChanged(nameof(ThemeSectionLabel));
        OnPropertyChanged(nameof(AppearanceLightLabel));
        OnPropertyChanged(nameof(AppearanceDarkLabel));
        OnPropertyChanged(nameof(AppearanceSystemLabel));
        OnPropertyChanged(nameof(ColorSchemeSectionLabel));
        OnPropertyChanged(nameof(LanguageTitle));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(RefreshIntervalLabel));
        OnPropertyChanged(nameof(WindowWidthLabel));
        OnPropertyChanged(nameof(WindowHeightLabel));
        OnPropertyChanged(nameof(WindowHint));
        OnPropertyChanged(nameof(TrayTitle));
        OnPropertyChanged(nameof(TrayEnabledLabel));
        OnPropertyChanged(nameof(TrayCloseLabel));
        OnPropertyChanged(nameof(TrayMinimizeLabel));
        OnPropertyChanged(nameof(TorrentAssociationTitle));
        OnPropertyChanged(nameof(RegisterAssociationLabel));
        OnPropertyChanged(nameof(LanguageEnglishLabel));
        OnPropertyChanged(nameof(LanguageRussianLabel));
        OnPropertyChanged(nameof(LanguageGermanLabel));
        OnPropertyChanged(nameof(LanguageFrenchLabel));
    }

    private string T(string key) => _localization.T(key);
}

internal sealed partial class PaletteChipViewModel : ViewModelBase
{
    public string Id { get; init; } = string.Empty;
    public string ColorHex { get; init; } = string.Empty;

    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private bool _isSelected;
}
