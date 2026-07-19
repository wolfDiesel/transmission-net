using TransmissonNET.Infrastructure.TorrentProviders;
using TransmissonNET.Providers.Abstractions;
using Xunit;

namespace TransmissonNET.Infrastructure.Tests;

public sealed class TorrentProviderLoaderTests
{
    [Fact]
    public void LoadFromDirectory_WhenMissing_ReturnsEmptyCatalog()
    {
        var dir = Path.Combine(Path.GetTempPath(), "tn-providers-missing-" + Guid.NewGuid().ToString("N"));
        var catalog = TorrentProviderLoader.LoadFromDirectory(dir);

        Assert.Empty(catalog.GetProviders());
        Assert.Empty(catalog.LoadErrors);
    }

    [Fact]
    public void LoadFromDirectory_LoadsFakeProviderDll()
    {
        var fakeDll = FindFakeProviderDll();
        Assert.True(File.Exists(fakeDll), $"Fake provider DLL not found: {fakeDll}");

        var dir = Path.Combine(Path.GetTempPath(), "tn-providers-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var dest = Path.Combine(dir, Path.GetFileName(fakeDll));
            File.Copy(fakeDll, dest, overwrite: true);

            var abstractions = typeof(ITorrentProvider).Assembly.Location;
            if (!string.IsNullOrWhiteSpace(abstractions) && File.Exists(abstractions))
            {
                File.Copy(
                    abstractions,
                    Path.Combine(dir, Path.GetFileName(abstractions)),
                    overwrite: true);
            }

            var catalog = TorrentProviderLoader.LoadFromDirectory(dir);

            Assert.Empty(catalog.LoadErrors);
            var provider = Assert.Single(catalog.GetProviders());
            Assert.Equal("fake", provider.Id);
            Assert.Equal("Fake Provider", provider.DisplayName);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    private static string FindFakeProviderDll()
    {
        var probe = new DirectoryInfo(AppContext.BaseDirectory);
        while (probe is not null)
        {
            var candidate = Path.Combine(
                probe.FullName,
                "tests",
                "TransmissonNET.Providers.Fake",
                "bin",
                "Debug",
                "net8.0",
                "TransmissonNET.Providers.Fake.dll");
            if (File.Exists(candidate))
                return candidate;

            candidate = Path.Combine(
                probe.FullName,
                "tests",
                "TransmissonNET.Providers.Fake",
                "bin",
                "Release",
                "net8.0",
                "TransmissonNET.Providers.Fake.dll");
            if (File.Exists(candidate))
                return candidate;

            probe = probe.Parent;
        }

        return Path.Combine(
            AppContext.BaseDirectory,
            "TransmissonNET.Providers.Fake.dll");
    }
}
