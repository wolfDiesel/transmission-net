using System.Collections.ObjectModel;

namespace TransmissonNET.Providers.Abstractions;

public interface ITorrentProvider
{
    string Id { get; }

    string DisplayName { get; }

    bool IsLoginRequired { get; }

    bool IsLoggedIn { get; }

    ObservableCollection<TorrentSearchHit> Results { get; }

    IReadOnlyList<string> KnownMirrors { get; }

    Task LoginAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task SearchAsync(string query, CancellationToken cancellationToken = default);

    Task<byte[]> DownloadTorrentAsync(string hitId, CancellationToken cancellationToken = default);

    TorrentProviderSettings GetSettings();

    void SetSettings(TorrentProviderSettings settings);
}

public static class TorrentProviderUiMarshal
{
    public static void Run(SynchronizationContext? context, Action action)
    {
        if (context is null || SynchronizationContext.Current == context)
        {
            action();
            return;
        }

        context.Send(_ => action(), null);
    }

    public static void ClearResults(ObservableCollection<TorrentSearchHit> results, SynchronizationContext? context) =>
        Run(context, results.Clear);

    public static void ReplaceResults(
        ObservableCollection<TorrentSearchHit> results,
        IEnumerable<TorrentSearchHit> hits,
        SynchronizationContext? context)
    {
        Run(context, () =>
        {
            results.Clear();
            foreach (var hit in hits)
                results.Add(hit);
        });
    }
}
