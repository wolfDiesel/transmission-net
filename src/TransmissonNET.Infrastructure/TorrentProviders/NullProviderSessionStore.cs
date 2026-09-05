using System.Net;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Infrastructure.TorrentProviders;

/// <summary>
/// No-op facade for <see cref="IProviderSessionStore"/>. Provider clients keep
/// managing their own cookie files until F6 replaces the storage with a secure one.
/// </summary>
public sealed class NullProviderSessionStore : IProviderSessionStore
{
    public int LoadInto(CookieContainer container) => 0;

    public void Save(CookieContainer container, string? userAgent = null)
    {
    }

    public void Clear()
    {
    }

    public string? UserAgent => null;
}