using System.Net;

namespace TransmissonNET.Providers.Abstractions;

/// <summary>
/// Persists and restores a provider's web session (cookies + user agent) on disk.
/// Declared here so provider plugins do not need a reference to the host assembly.
/// </summary>
public interface IProviderSessionStore
{
    /// <summary>Loads stored cookies into <paramref name="container"/>; returns the count loaded.</summary>
    int LoadInto(CookieContainer container);

    /// <summary>Saves the given cookies and optional user agent.</summary>
    void Save(CookieContainer container, string? userAgent = null);

    /// <summary>Clears any stored session data.</summary>
    void Clear();

    /// <summary>The persisted user agent, if any.</summary>
    string? UserAgent { get; }
}