using TransmissonNET.Application.Abstractions;

namespace TransmissonNET.Infrastructure.Desktop;

public sealed class LinuxTorrentFileAssociationService : ITorrentFileAssociationService
{
    public const string DesktopFileName = "transmission-net.desktop";
    public const string ApplicationName = "TransmissionNET";
    public const string StartupWmClass = "TransmissionNET";
    internal const string AppExecutableBaseName = "TransmissonNET.App";
    private const string MimeType = "application/x-bittorrent";

    public bool IsSupported => OperatingSystem.IsLinux();

    public bool IsDefaultHandler()
    {
        if (!IsSupported)
            return false;

        var defaultFile = QueryDefaultDesktopId();
        if (string.IsNullOrWhiteSpace(defaultFile))
            return false;

        var execPath = ResolveExecutablePath();
        var matches = FindMatchingDesktopEntries(GetApplicationsDirectory(), execPath);
        if (matches.Count > 0)
            return matches.Any(entry => defaultFile.Equals(Path.GetFileName(entry.FilePath), StringComparison.OrdinalIgnoreCase));

        return defaultFile.Equals(DesktopFileName, StringComparison.OrdinalIgnoreCase);
    }

    public bool HasDesktopEntry()
    {
        if (!IsSupported)
            return false;

        return FindMatchingDesktopEntries(GetApplicationsDirectory(), ResolveExecutablePath()).Count > 0;
    }

    public async Task RegisterAsync(CancellationToken cancellationToken = default)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("Linux desktop integration is required.");

        var execPath = ResolveExecutablePath();
        if (!File.Exists(execPath))
            throw new InvalidOperationException($"Application executable was not found: {execPath}");

        var applicationsDir = GetApplicationsDirectory();
        Directory.CreateDirectory(applicationsDir);

        var canonicalPath = Path.Combine(applicationsDir, DesktopFileName);
        var matches = FindMatchingDesktopEntries(applicationsDir, execPath);
        var targetPaths = matches
            .Where(entry => !IsAppImageManagerDesktop(entry.FilePath))
            .Select(entry => entry.FilePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        targetPaths.Add(canonicalPath);

        var desktopContent = BuildDesktopEntry(execPath);
        foreach (var desktopPath in targetPaths)
            await File.WriteAllTextAsync(desktopPath, desktopContent, DesktopFileEncoding.Instance, cancellationToken);

        TryInstallIcon();
        UpdateDesktopDatabase(applicationsDir);
        ValidateDesktopEntry(canonicalPath);
        ApplyDefaultMimeHandler(DesktopFileName);

        if (!IsDefaultHandler())
        {
            throw new InvalidOperationException(
                "Desktop entry was written, but the system did not switch the default .torrent handler. "
                + "Open Settings → Applications → Default applications and choose TransmissionNET for torrent files.");
        }
    }

    internal static IReadOnlyList<ParsedDesktopEntry> FindMatchingDesktopEntries(
        string applicationsDir,
        string execPath) =>
        TransmissionNetDesktopMatcher.FindMatches(applicationsDir, execPath);

    internal static string? FindExistingDesktopEntryPath(string applicationsDir, string execPath)
    {
        var matches = FindMatchingDesktopEntries(applicationsDir, execPath);
        return matches.Count > 0 ? matches[0].FilePath : null;
    }

    internal static string ResolveDesktopEntryPathForWrite(string applicationsDir, string execPath) =>
        FindExistingDesktopEntryPath(applicationsDir, execPath)
        ?? Path.Combine(applicationsDir, DesktopFileName);

    internal static string ResolvePrimaryDesktopEntryPath(IReadOnlyList<string> targetPaths)
    {
        var canonical = targetPaths.FirstOrDefault(path =>
            Path.GetFileName(path).Equals(DesktopFileName, StringComparison.OrdinalIgnoreCase));
        return canonical ?? targetPaths[0];
    }

    internal static string BuildDesktopEntry(string execPath)
    {
        var escapedExec = execPath.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return string.Join(
            '\n',
            "[Desktop Entry]",
            "Version=1.0",
            "Type=Application",
            $"Name={ApplicationName}",
            "Comment=Desktop client for Transmission RPC",
            $"Exec=\"{escapedExec}\" %f",
            "Icon=transmission-net",
            "Categories=Network;FileTransfer;",
            $"StartupWMClass={StartupWmClass}",
            "Terminal=false",
            $"MimeType={MimeType};",
            "") + '\n';
    }

    internal static string ResolveExecutablePath()
    {
        foreach (var candidate in EnumerateExecutableCandidates())
        {
            if (string.IsNullOrWhiteSpace(candidate) || !File.Exists(candidate))
                continue;

            var fullPath = Path.GetFullPath(candidate);
            if (IsEphemeralAppImageMountPath(fullPath))
                continue;

            return fullPath;
        }

        var fallback = Path.GetFullPath(Environment.ProcessPath ?? AppExecutableBaseName);
        return fallback;
    }

    private static IEnumerable<string?> EnumerateExecutableCandidates()
    {
        yield return Environment.GetEnvironmentVariable("APPIMAGE");
        yield return Environment.GetEnvironmentVariable("ARGV0");

        foreach (var procPath in TryResolveProcExePath())
            yield return procPath;

        var baseDir = AppContext.BaseDirectory;
        yield return Path.Combine(baseDir, AppExecutableBaseName);
        yield return Environment.ProcessPath;
    }

    private static IEnumerable<string> TryResolveProcExePath()
    {
        try
        {
            var procExe = Path.Combine("/proc/self", "exe");
            if (!File.Exists(procExe))
                return [];

            var link = File.ResolveLinkTarget(procExe, returnFinalTarget: true);
            return link is null ? [] : [link.FullName];
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    private static bool IsEphemeralAppImageMountPath(string path) =>
        path.Contains("/.mount_", StringComparison.Ordinal);

    private static string GetApplicationsDirectory() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "share",
            "applications");

    private static string? QueryDefaultDesktopId() =>
        DesktopProcessRunner.TryRun("xdg-mime", "query", "default", MimeType);

    private static void UpdateDesktopDatabase(string applicationsDir)
    {
        var result = DesktopProcessRunner.Run("update-desktop-database", applicationsDir);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(result.StdErr)
                    ? "update-desktop-database failed."
                    : $"update-desktop-database failed: {result.StdErr}");
        }
    }

    internal static bool IsAppImageManagerDesktop(string filePath) =>
        Path.GetFileName(filePath).StartsWith("appimagemanager-", StringComparison.OrdinalIgnoreCase);

    private static void ValidateDesktopEntry(string desktopPath)
    {
        var bytes = File.ReadAllBytes(desktopPath);
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            throw new InvalidOperationException("Desktop entry must not contain a UTF-8 BOM.");

        if (!IsDesktopFileValidateAvailable())
            return;

        var validate = DesktopProcessRunner.Run("desktop-file-validate", desktopPath);
        if (validate.ExitCode == 0)
            return;

        var details = string.IsNullOrWhiteSpace(validate.StdErr) ? validate.StdOut : validate.StdErr;
        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(details)
                ? "Desktop entry validation failed."
                : $"Desktop entry validation failed: {details}");
    }

    private static bool IsDesktopFileValidateAvailable() =>
        File.Exists("/usr/bin/desktop-file-validate") || File.Exists("/bin/desktop-file-validate");

    private static void TryInstallIcon()
    {
        var iconSource = FindBundledIconPath();
        if (iconSource is null)
            return;

        var targetDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "share",
            "icons",
            "hicolor",
            "scalable",
            "apps");
        Directory.CreateDirectory(targetDir);
        File.Copy(iconSource, Path.Combine(targetDir, "transmission-net.svg"), overwrite: true);
    }

    private static string? FindBundledIconPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "transmission-net.svg"),
            Path.Combine(AppContext.BaseDirectory, "..", "transmission-net.svg"),
            Environment.GetEnvironmentVariable("APPDIR") is { Length: > 0 } appDir
                ? Path.Combine(appDir, "transmission-net.svg")
                : null,
        };

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            try
            {
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            catch (ArgumentException)
            {
            }
        }

        return null;
    }

    private static void ApplyDefaultMimeHandler(string desktopFileName)
    {
        var failures = new List<string>();

        var xdg = DesktopProcessRunner.Run("xdg-mime", "default", desktopFileName, MimeType);
        if (xdg.ExitCode == 0 && IsDefaultHandlerForDesktop(desktopFileName))
            return;

        if (xdg.ExitCode != 0 && !string.IsNullOrWhiteSpace(xdg.StdErr))
            failures.Add($"xdg-mime: {xdg.StdErr}");

        var gio = DesktopProcessRunner.Run("gio", "mime", MimeType, desktopFileName);
        if (gio.ExitCode == 0 && IsDefaultHandlerForDesktop(desktopFileName))
            return;

        if (gio.ExitCode != 0 && !string.IsNullOrWhiteSpace(gio.StdErr))
            failures.Add($"gio: {gio.StdErr}");

        try
        {
            MimeAppsListWriter.SetDefaultHandler(MimeType, desktopFileName);
            if (IsDefaultHandlerForDesktop(desktopFileName))
                return;
        }
        catch (Exception ex)
        {
            failures.Add($"mimeapps.list: {ex.Message}");
        }

        var current = QueryDefaultDesktopId() ?? "(none)";
        failures.Add($"current default: {current}");
        throw new InvalidOperationException(
            "Could not set TransmissionNET as the default .torrent handler. "
            + string.Join(" ", failures));
    }

    private static bool IsDefaultHandlerForDesktop(string desktopFileName)
    {
        var current = QueryDefaultDesktopId();
        return !string.IsNullOrWhiteSpace(current)
            && current.Equals(desktopFileName, StringComparison.OrdinalIgnoreCase);
    }
}
