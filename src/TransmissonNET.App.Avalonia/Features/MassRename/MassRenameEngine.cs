using System.Text.RegularExpressions;
using TransmissonNET.Application.Contracts;

namespace TransmissonNET.App.Avalonia.Features.MassRename;

internal static class MassRenameEngine
{
    private static readonly Regex IllegalNameChars = new(@"[\\/:*?""<>|]");

    public static MassRenameRule DefaultRule() =>
        new()
        {
            Mode = MassRenameMode.Regex,
            NumberingTemplate = "{n:02} - {name}",
            NumberingStart = 1,
            NumberingStep = 1,
            RegexFlags = "g",
            Template = "{n:02} - {name}",
            StemOnly = true,
            Sort = MassRenameSort.Path,
        };

    public static string FormatScopeLabel(string scopePath)
    {
        var normalized = NormalizeScopePath(scopePath);
        return string.IsNullOrEmpty(normalized) ? "All files" : $"{normalized}/";
    }

    public static IReadOnlyList<ScopeFile> CollectScopeFiles(
        IReadOnlyList<TorrentFileNodeDto> fileTree,
        string scopePath)
    {
        var normalized = NormalizeScopePath(scopePath);
        var files = new List<ScopeFile>();
        Walk(fileTree, normalized, files);
        return files.OrderBy(f => f.Path, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<RenamePlanEntry> BuildRenamePlan(IReadOnlyList<ScopeFile> files, MassRenameRule rule)
    {
        var sorted = SortFiles(files, rule.Sort);
        return sorted.Select((file, index) =>
        {
            var newName = ComputeNewBasename(file, index, rule);
            return new RenamePlanEntry
            {
                Path = file.Path,
                OldName = file.Basename,
                NewName = newName,
                Changed = !string.Equals(newName, file.Basename, StringComparison.Ordinal),
            };
        }).ToList();
    }

    public static IReadOnlyList<string> ValidateMassRenameRule(MassRenameRule rule, IReadOnlyList<ScopeFile> files)
    {
        if (rule.Mode != MassRenameMode.Regex)
            return Array.Empty<string>();

        var (ok, regex, error) = MassRenameRegex.Compile(rule.RegexPattern, rule.RegexFlags);
        if (!ok || regex is null)
            return [error];

        var hasMatch = files.Any(file => MassRenameRegex.Matches(regex, file.Basename));
        if (!hasMatch)
        {
            return
            [
                "Pattern matches no files in scope. Regex is applied to the full file name (with extension), e.g. Genocyber 01 ….mkv",
            ];
        }

        return Array.Empty<string>();
    }

    public static PlanValidation ValidatePlan(IReadOnlyList<RenamePlanEntry> plan)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var changed = plan.Where(entry => entry.Changed).ToList();

        if (changed.Count == 0)
            warnings.Add("No files would be renamed with the current rules.");

        var targetByFolder = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.Ordinal);

        foreach (var entry in changed)
        {
            if (string.IsNullOrWhiteSpace(entry.NewName))
            {
                errors.Add($"Empty name for {entry.Path}");
                continue;
            }

            if (IllegalNameChars.IsMatch(entry.NewName))
                errors.Add($"Invalid characters in \"{entry.NewName}\" ({entry.Path})");

            var folder = ParentPath(entry.Path);
            if (!targetByFolder.TryGetValue(folder, out var folderMap))
            {
                folderMap = new Dictionary<string, List<string>>(StringComparer.Ordinal);
                targetByFolder[folder] = folderMap;
            }

            if (!folderMap.TryGetValue(entry.NewName, out var paths))
            {
                paths = [];
                folderMap[entry.NewName] = paths;
            }

            paths.Add(entry.Path);
        }

        foreach (var (folder, names) in targetByFolder)
        {
            foreach (var (name, paths) in names)
            {
                if (paths.Count <= 1)
                    continue;
                var label = string.IsNullOrEmpty(folder) ? string.Empty : $"{folder}/";
                errors.Add($"Duplicate name \"{label}{name}\" for {paths.Count} files");
            }
        }

        if (changed.Count > 200)
            warnings.Add($"{changed.Count} files will be renamed. This may take a while.");

        return new PlanValidation
        {
            CanApply = errors.Count == 0 && changed.Count > 0,
            Errors = errors,
            Warnings = warnings,
        };
    }

    private static void Walk(IReadOnlyList<TorrentFileNodeDto> nodes, string scopePath, List<ScopeFile> output)
    {
        foreach (var node in nodes)
        {
            if (node.IsFolder)
            {
                Walk(node.Children, scopePath, output);
                continue;
            }

            if (!IsInScope(node.Path, scopePath))
                continue;

            var basename = node.Name;
            var (stem, ext) = SplitBasename(basename);
            var relativePath = string.IsNullOrEmpty(scopePath)
                ? node.Path
                : node.Path[(scopePath.Length + 1)..];

            output.Add(new ScopeFile
            {
                Path = node.Path,
                Basename = basename,
                Stem = stem,
                Ext = ext,
                RelativePath = relativePath,
            });
        }
    }

    private static string NormalizeScopePath(string scopePath) =>
        scopePath.Trim().Replace('\\', '/').Trim('/');

    private static bool IsInScope(string path, string scopePath)
    {
        if (string.IsNullOrEmpty(scopePath))
            return true;
        return path == scopePath || path.StartsWith($"{scopePath}/", StringComparison.Ordinal);
    }

    private static (string Stem, string Ext) SplitBasename(string basename)
    {
        if (string.IsNullOrEmpty(basename))
            return (string.Empty, string.Empty);
        var dot = basename.LastIndexOf('.');
        if (dot <= 0)
            return (basename, string.Empty);
        return (basename[..dot], basename[dot..]);
    }

    private static string ParentPath(string path)
    {
        var idx = path.LastIndexOf('/');
        return idx < 0 ? string.Empty : path[..idx];
    }

    private static IReadOnlyList<ScopeFile> SortFiles(IReadOnlyList<ScopeFile> files, MassRenameSort sort) =>
        sort == MassRenameSort.Name
            ? files.OrderBy(f => f.Basename, StringComparer.Ordinal).ToList()
            : files.OrderBy(f => f.Path, StringComparer.Ordinal).ToList();

    private static string ComputeNewBasename(ScopeFile file, int index, MassRenameRule rule)
    {
        var target = rule.StemOnly ? file.Stem : file.Basename;
        var result = rule.Mode switch
        {
            MassRenameMode.FindReplace => ApplyFindReplace(target, rule.Find, rule.Replace, rule.CaseSensitive),
            MassRenameMode.PrefixSuffix => $"{rule.Prefix}{target}{rule.Suffix}",
            MassRenameMode.Numbering => ApplyNumberingTemplate(rule.NumberingTemplate, index, rule.NumberingStart, rule.NumberingStep),
            MassRenameMode.Regex => ApplyRegex(file.Basename, rule.RegexPattern, rule.RegexReplacement, rule.RegexFlags),
            MassRenameMode.Template => ApplyTemplate(rule.Template, file, index, rule),
            _ => target,
        };

        if (rule.StemOnly && rule.Mode != MassRenameMode.Regex)
            return $"{result}{file.Ext}";
        return result;
    }

    private static string ApplyFindReplace(string text, string find, string replace, bool caseSensitive)
    {
        if (string.IsNullOrEmpty(find))
            return text;
        return caseSensitive
            ? text.Replace(find, replace)
            : Regex.Replace(text, Regex.Escape(find), replace, RegexOptions.IgnoreCase);
    }

    private static string ApplyRegex(string text, string pattern, string replacement, string flags)
    {
        var (ok, regex, _) = MassRenameRegex.Compile(pattern, flags);
        if (!ok || regex is null)
            return text;
        return regex.Replace(text, replacement);
    }

    private static string ApplyNumberingTemplate(string template, int index, int start, int step)
    {
        var n = start + index * step;
        return Regex.Replace(template, @"\{n(?::(\d+))?\}", match =>
        {
            if (match.Groups[1].Success)
                return n.ToString().PadLeft(int.Parse(match.Groups[1].Value), '0');
            return n.ToString();
        });
    }

    private static string ApplyTemplate(string template, ScopeFile file, int index, MassRenameRule rule)
    {
        var n = rule.NumberingStart + index * rule.NumberingStep;
        var result = (string.IsNullOrWhiteSpace(template) ? "{name}" : template)
            .Replace("{name}", file.Stem, StringComparison.Ordinal)
            .Replace("{ext}", file.Ext, StringComparison.Ordinal)
            .Replace("{basename}", file.Basename, StringComparison.Ordinal)
            .Replace("{path}", file.RelativePath, StringComparison.Ordinal);
        return Regex.Replace(result, @"\{n(?::(\d+))?\}", match =>
        {
            if (match.Groups[1].Success)
                return n.ToString().PadLeft(int.Parse(match.Groups[1].Value), '0');
            return n.ToString();
        });
    }
}
