using System.Diagnostics;
using System.Text;
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

        var output = RunProcess("xdg-mime", "query", "default", MimeType);
        if (string.IsNullOrWhiteSpace(output))
            return false;

        var defaultFile = output.Trim();
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
        var applicationsDir = GetApplicationsDirectory();
        Directory.CreateDirectory(applicationsDir);

        var matches = FindMatchingDesktopEntries(applicationsDir, execPath);
        var targetPaths = matches.Count > 0
            ? matches.Select(entry => entry.FilePath).ToList()
            : [Path.Combine(applicationsDir, DesktopFileName)];

        var desktopContent = BuildDesktopEntry(execPath);
        foreach (var desktopPath in targetPaths)
            await File.WriteAllTextAsync(desktopPath, desktopContent, Encoding.UTF8, cancellationToken);

        RunProcess("update-desktop-database", applicationsDir);
        var defaultDesktopFile = Path.GetFileName(ResolvePrimaryDesktopEntryPath(targetPaths));
        RunProcess("xdg-mime", "default", MimeType, defaultDesktopFile);
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
        return $"""
            [Desktop Entry]
            Type=Application
            Name={ApplicationName}
            Comment=Desktop client for Transmission RPC
            Exec="{escapedExec}" %f
            Icon=transmission-net
            Categories=Network;FileTransfer;
            StartupWMClass={StartupWmClass}
            Terminal=false
            MimeType=application/x-bittorrent;
            """;
    }

    internal static string ResolveExecutablePath()
    {
        var appImage = Environment.GetEnvironmentVariable("APPIMAGE");
        if (!string.IsNullOrWhiteSpace(appImage) && File.Exists(appImage))
            return Path.GetFullPath(appImage);

        var baseDir = AppContext.BaseDirectory;
        var bundled = Path.Combine(baseDir, AppExecutableBaseName);
        if (File.Exists(bundled))
            return Path.GetFullPath(bundled);

        return Path.GetFullPath(Environment.ProcessPath ?? bundled);
    }

    private static string GetApplicationsDirectory() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "share",
            "applications");

    private static string RunProcess(string fileName, params string[] args)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        foreach (var arg in args)
            process.StartInfo.ArgumentList.Add(arg);

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output.Trim();
    }
}
