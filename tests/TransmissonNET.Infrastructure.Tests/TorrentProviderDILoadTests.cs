using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Infrastructure.TorrentProviders;
using TransmissonNET.Providers.Abstractions;
using Xunit;

namespace TransmissonNET.Infrastructure.Tests;

/// <summary>
/// Verifies that real provider plugins (RuTracker/LostFilm/Kinozal) can be
/// constructed through the DI-aware load path with stub UI/session services.
/// </summary>
public sealed class TorrentProviderDILoadTests
{
    [Fact]
    public void LoadFromDirectory_ResolvesRealProvidersWithStubDependencies()
    {
        var baseDir = AppContext.BaseDirectory;
        var providerDlls = new[]
        {
            "TransmissonNET.Providers.RuTracker.dll",
            "TransmissonNET.Providers.LostFilm.dll",
            "TransmissonNET.Providers.Kinozal.dll",
        };

        var providersDir = Path.Combine(Path.GetTempPath(), "tn-providers-di-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(providersDir);
        try
        {
            foreach (var dll in providerDlls)
            {
                var src = Path.Combine(baseDir, dll);
                if (File.Exists(src))
                    File.Copy(src, Path.Combine(providersDir, dll), overwrite: true);
            }

            // Abstractions must be resolvable by the plugin load context.
            var abstractions = typeof(ITorrentProvider).Assembly.Location;
            if (!string.IsNullOrWhiteSpace(abstractions) && File.Exists(abstractions))
            {
                File.Copy(
                    abstractions,
                    Path.Combine(providersDir, Path.GetFileName(abstractions)),
                    overwrite: true);
            }

            var services = new ServiceCollection();
            services.AddSingleton<IProviderUiHost, StubProviderUiHost>();
            services.AddSingleton<IProviderSessionStore, StubProviderSessionStore>();
            services.AddTransient<TorrentProviderSettings>();
            using var sp = services.BuildServiceProvider();

            var catalog = TorrentProviderLoader.LoadFromDirectory(providersDir, sp);

            Assert.DoesNotContain(
                catalog.LoadErrors,
                e => e.Contains("RuTracker", StringComparison.OrdinalIgnoreCase)
                     || e.Contains("LostFilm", StringComparison.OrdinalIgnoreCase)
                     || e.Contains("Kinozal", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(catalog.GetProviders(), p => p.Id == "rutracker");
            Assert.Contains(catalog.GetProviders(), p => p.Id == "lostfilm");
            Assert.Contains(catalog.GetProviders(), p => p.Id == "kinozal");
        }
        finally
        {
            if (Directory.Exists(providersDir))
                Directory.Delete(providersDir, recursive: true);
        }
    }

    private sealed class StubProviderUiHost : IProviderUiHost
    {
        public Task<ProviderLoginResult?> LoginAsync(
            string providerId,
            string baseUrl,
            string dataDirectory,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ProviderLoginResult?>(new ProviderLoginResult([], null));
    }

    private sealed class StubProviderSessionStore : IProviderSessionStore
    {
        public int LoadInto(System.Net.CookieContainer container) => 0;

        public void Save(System.Net.CookieContainer container, string? userAgent = null)
        {
        }

        public void Clear()
        {
        }

        public string? UserAgent => null;
    }
}