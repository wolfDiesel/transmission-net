using System.Reflection;
using System.Runtime.Loader;
using TransmissonNET.Application.Abstractions;
using TransmissonNET.Providers.Abstractions;

namespace TransmissonNET.Infrastructure.TorrentProviders;

public sealed class TorrentProviderCatalog : ITorrentProviderCatalog
{
    private readonly IReadOnlyList<ITorrentProvider> _providers;
    private readonly IReadOnlyList<string> _loadErrors;

    public TorrentProviderCatalog(IEnumerable<ITorrentProvider> providers, IEnumerable<string>? loadErrors = null)
    {
        _providers = providers.ToList();
        _loadErrors = (loadErrors ?? []).ToList();
    }

    public IReadOnlyList<string> LoadErrors => _loadErrors;

    public IReadOnlyList<ITorrentProvider> GetProviders() => _providers;

    public ITorrentProvider? GetById(string providerId) =>
        _providers.FirstOrDefault(p =>
            string.Equals(p.Id, providerId, StringComparison.OrdinalIgnoreCase));
}

public static class TorrentProviderLoader
{
    public static TorrentProviderCatalog LoadFromDirectory(string providersDirectory)
    {
        var providers = new List<ITorrentProvider>();
        var errors = new List<string>();

        if (!Directory.Exists(providersDirectory))
            return new TorrentProviderCatalog(providers, errors);

        foreach (var dllPath in Directory.EnumerateFiles(providersDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                     .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var fileName = Path.GetFileName(dllPath);
            if (fileName.StartsWith("TransmissonNET.Providers.Abstractions", StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                var assembly = LoadPluginAssembly(dllPath);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface || !typeof(ITorrentProvider).IsAssignableFrom(type))
                        continue;

                    var ctor = type.GetConstructor(Type.EmptyTypes);
                    if (ctor is null)
                    {
                        errors.Add($"{fileName}: {type.FullName} has no public parameterless constructor.");
                        continue;
                    }

                    if (Activator.CreateInstance(type) is ITorrentProvider provider)
                        providers.Add(provider);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var detail = string.Join("; ", ex.LoaderExceptions.Select(e => e?.Message).Where(m => !string.IsNullOrWhiteSpace(m)));
                errors.Add($"{fileName}: {detail}");
            }
            catch (Exception ex)
            {
                errors.Add($"{fileName}: {ex.Message}");
            }
        }

        return new TorrentProviderCatalog(providers, errors);
    }

    private static Assembly LoadPluginAssembly(string dllPath)
    {
        var context = new ProviderLoadContext(dllPath);
        return context.LoadFromAssemblyPath(Path.GetFullPath(dllPath));
    }

    private sealed class ProviderLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public ProviderLoadContext(string pluginPath)
            : base(isCollectible: false)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var existing = Default.Assemblies.FirstOrDefault(a =>
                string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
                return existing;

            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            return path is null ? null : LoadFromAssemblyPath(path);
        }
    }
}
