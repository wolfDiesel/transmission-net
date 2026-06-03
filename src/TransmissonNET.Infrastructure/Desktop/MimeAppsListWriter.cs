using System.Text;

namespace TransmissonNET.Infrastructure.Desktop;

internal static class MimeAppsListWriter
{
    private const string DefaultApplicationsSection = "[Default Applications]";

    public static void SetDefaultHandler(string mimeType, string desktopFileName)
    {
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config");
        Directory.CreateDirectory(configDir);

        var mimeAppsPath = Path.Combine(configDir, "mimeapps.list");
        var lines = File.Exists(mimeAppsPath)
            ? File.ReadAllLines(mimeAppsPath).ToList()
            : new List<string>();

        var assignment = $"{mimeType}={desktopFileName};";
        var sectionIndex = lines.FindIndex(line =>
            line.Trim().Equals(DefaultApplicationsSection, StringComparison.Ordinal));

        if (sectionIndex < 0)
        {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add(string.Empty);
            lines.Add(DefaultApplicationsSection);
            lines.Add(assignment);
        }
        else
        {
            var replaced = false;
            for (var i = sectionIndex + 1; i < lines.Count; i++)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.StartsWith('['))
                    break;

                if (!trimmed.StartsWith($"{mimeType}=", StringComparison.Ordinal))
                    continue;

                lines[i] = assignment;
                replaced = true;
                break;
            }

            if (!replaced)
                lines.Insert(sectionIndex + 1, assignment);
        }

        File.WriteAllLines(mimeAppsPath, lines, DesktopFileEncoding.Instance);
    }
}
