using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class SearchProviderTagViewModel : ViewModelBase
{
    private readonly ITorrentProvider _provider;
    private readonly Action<SearchProviderTagViewModel> _remove;
    private readonly Func<SearchProviderTagViewModel, Task> _logout;

    public SearchProviderTagViewModel(
        ITorrentProvider provider,
        string logoutText,
        Action<SearchProviderTagViewModel> remove,
        Func<SearchProviderTagViewModel, Task> logout)
    {
        _provider = provider;
        Id = provider.Id;
        DisplayName = provider.DisplayName;
        LogoutText = logoutText;
        _remove = remove;
        _logout = logout;
        RefreshAuthState();
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string LogoutText { get; private set; }

    public bool ShowAuthIndicator { get; private set; }

    [ObservableProperty] private bool _isLoggedIn;

    [ObservableProperty] private bool _showLogout;

    public void SetLogoutText(string text)
    {
        LogoutText = text;
        OnPropertyChanged(nameof(LogoutText));
    }

    public void RefreshAuthState()
    {
        ShowAuthIndicator = _provider.IsLoginRequired;
        IsLoggedIn = _provider.IsLoginRequired && _provider.IsLoggedIn;
        ShowLogout = IsLoggedIn;
        OnPropertyChanged(nameof(ShowAuthIndicator));
    }

    [RelayCommand]
    private void Remove() => _remove(this);

    [RelayCommand]
    private async Task LogoutAsync() => await _logout(this);
}

internal sealed partial class SearchProviderOptionViewModel : ViewModelBase
{
    private readonly Action<SearchProviderOptionViewModel> _add;

    public SearchProviderOptionViewModel(
        string id,
        string displayName,
        Action<SearchProviderOptionViewModel> add)
    {
        Id = id;
        DisplayName = displayName;
        _add = add;
    }

    public string Id { get; }

    public string DisplayName { get; }

    [RelayCommand]
    private void Add() => _add(this);
}
