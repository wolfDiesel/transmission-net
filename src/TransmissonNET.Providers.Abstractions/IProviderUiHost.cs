using System.Net;

namespace TransmissonNET.Providers.Abstractions;

/// <summary>
/// Session/login result produced by the host UI (login window / web login).
/// </summary>
public sealed record ProviderLoginResult(
    IReadOnlyList<Cookie> Cookies,
    string? UserAgent,
    string? SessionCookie = null,
    string? Email = null,
    string? Password = null);

public interface IProviderUiHost
{
    /// <summary>
    /// Shows the provider-specific login UI (window/browser) and returns the
    /// resulting session, or <see langword="null"/> if the user cancelled.
    /// </summary>
    Task<ProviderLoginResult?> LoginAsync(
        string providerId,
        string baseUrl,
        string dataDirectory,
        CancellationToken cancellationToken = default);
}