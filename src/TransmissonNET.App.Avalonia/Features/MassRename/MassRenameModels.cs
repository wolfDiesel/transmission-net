namespace TransmissonNET.App.Avalonia.Features.MassRename;

internal enum MassRenameMode
{
    FindReplace,
    PrefixSuffix,
    Numbering,
    Regex,
    Template,
}

internal enum MassRenameSort
{
    Path,
    Name,
}

internal sealed class ScopeFile
{
    public required string Path { get; init; }
    public required string Basename { get; init; }
    public required string Stem { get; init; }
    public required string Ext { get; init; }
    public required string RelativePath { get; init; }
}

internal sealed class RenamePlanEntry
{
    public required string Path { get; init; }
    public required string OldName { get; init; }
    public required string NewName { get; init; }
    public bool Changed { get; init; }
}

internal sealed class MassRenameRule
{
    public MassRenameMode Mode { get; set; } = MassRenameMode.PrefixSuffix;
    public string Find { get; set; } = string.Empty;
    public string Replace { get; set; } = string.Empty;
    public bool CaseSensitive { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public string NumberingTemplate { get; set; } = "{n}";
    public int NumberingStart { get; set; } = 1;
    public int NumberingStep { get; set; } = 1;
    public string RegexPattern { get; set; } = string.Empty;
    public string RegexReplacement { get; set; } = string.Empty;
    public string RegexFlags { get; set; } = string.Empty;
    public string Template { get; set; } = "{name}";
    public bool StemOnly { get; set; }
    public MassRenameSort Sort { get; set; } = MassRenameSort.Path;
}

internal sealed class PlanValidation
{
    public bool CanApply { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
