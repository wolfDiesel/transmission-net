using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TransmissonNET.Application.Contracts;
using TransmissonNET.Application.Handlers;
using TransmissonNET.App.Avalonia.Features.MassRename;
using TransmissonNET.App.Avalonia.Services;

namespace TransmissonNET.App.Avalonia.ViewModels;

internal sealed partial class MassRenameViewModel : ViewModelBase
{
    private const int PreviewRowLimit = 200;

    private static readonly MassRenameMode[] ModeOrder =
    [
        MassRenameMode.Regex,
        MassRenameMode.FindReplace,
        MassRenameMode.PrefixSuffix,
        MassRenameMode.Numbering,
        MassRenameMode.Template,
    ];

    private readonly int _torrentId;
    private readonly HandlerInvoker _handlers;
    private readonly LocalizationService _localization;
    private readonly IReadOnlyList<ScopeFile> _scopeFiles;
    private readonly DispatcherTimer _debounce = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private IReadOnlyList<RenamePlanEntry> _plan = Array.Empty<RenamePlanEntry>();

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _scopeDetail = string.Empty;
    [ObservableProperty] private int _selectedModeIndex;
    [ObservableProperty] private bool _stemOnly = true;
    [ObservableProperty] private MassRenameSort _sort = MassRenameSort.Path;
    [ObservableProperty] private string _find = string.Empty;
    [ObservableProperty] private string _replace = string.Empty;
    [ObservableProperty] private bool _caseSensitive;
    [ObservableProperty] private string _prefix = string.Empty;
    [ObservableProperty] private string _suffix = string.Empty;
    [ObservableProperty] private string _numberingTemplate = "{n:02} - {name}";
    [ObservableProperty] private int _numberingStart = 1;
    [ObservableProperty] private int _numberingStep = 1;
    [ObservableProperty] private string _regexPattern = string.Empty;
    [ObservableProperty] private string _regexReplacement = string.Empty;
    [ObservableProperty] private string _regexFlags = "g";
    [ObservableProperty] private string _template = "{n:02} - {name}";
    [ObservableProperty] private string _generalSummary = string.Empty;
    [ObservableProperty] private string _helpTitle = string.Empty;
    [ObservableProperty] private string _helpSummary = string.Empty;
    [ObservableProperty] private string _generalStemSort = string.Empty;
    [ObservableProperty] private string _previewTitle = string.Empty;
    [ObservableProperty] private string _previewLimitedText = string.Empty;
    [ObservableProperty] private string _applyButtonText = string.Empty;
    [ObservableProperty] private bool _canApply;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _applied;
    [ObservableProperty] private int _sortIndex;

    public string CancelLabel { get; }
    public string ModeLabelRegex { get; }
    public string ModeLabelFindReplace { get; }
    public string ModeLabelPrefixSuffix { get; }
    public string ModeLabelNumbering { get; }
    public string ModeLabelTemplate { get; }
    public string StemOnlyLabel { get; }
    public string SortLabel { get; }
    public string FindLabel { get; }
    public string ReplaceLabel { get; }
    public string CaseSensitiveLabel { get; }
    public string PrefixLabel { get; }
    public string SuffixLabel { get; }
    public string TemplateLabel { get; }
    public string StartLabel { get; }
    public string StepLabel { get; }
    public string PatternLabel { get; }
    public string ReplacementLabel { get; }
    public string FlagsLabel { get; }
    public string OldNameLabel { get; }
    public string NewNameLabel { get; }
    public string NoFilesLabel { get; }
    public IReadOnlyList<string> SortOptionLabels { get; }
    public string NumberingPlaceholder { get; }
    public string TemplatePlaceholder { get; }

    public ObservableCollection<string> HelpExamples { get; } = new();
    public ObservableCollection<RenamePlanEntry> PreviewRows { get; } = new();
    public ObservableCollection<string> ValidationErrors { get; } = new();
    public ObservableCollection<string> ValidationWarnings { get; } = new();

    public bool IsRegexMode => SelectedModeIndex == 0;
    public bool IsFindReplaceMode => SelectedModeIndex == 1;
    public bool IsPrefixSuffixMode => SelectedModeIndex == 2;
    public bool IsNumberingMode => SelectedModeIndex == 3;
    public bool IsTemplateMode => SelectedModeIndex == 4;
    public bool HasPreviewRows => PreviewRows.Count > 0;
    public bool ShowPreviewLimited => !string.IsNullOrEmpty(PreviewLimitedText);

    public MassRenameViewModel(
        int torrentId,
        string scopePath,
        IReadOnlyList<TorrentFileNodeDto> fileTree,
        HandlerInvoker handlers,
        LocalizationService localization)
    {
        _torrentId = torrentId;
        _handlers = handlers;
        _localization = localization;
        _scopeFiles = MassRenameEngine.CollectScopeFiles(fileTree, scopePath);

        var defaults = MassRenameEngine.DefaultRule();
        StemOnly = defaults.StemOnly;
        Sort = defaults.Sort;
        NumberingTemplate = defaults.NumberingTemplate;
        NumberingStart = defaults.NumberingStart;
        NumberingStep = defaults.NumberingStep;
        RegexFlags = defaults.RegexFlags;
        Template = defaults.Template;
        SelectedModeIndex = 0;

        CancelLabel = localization.T("common.cancel");
        ModeLabelRegex = localization.T("massRename.modes.regex");
        ModeLabelFindReplace = localization.T("massRename.modes.findReplace");
        ModeLabelPrefixSuffix = localization.T("massRename.modes.prefixSuffix");
        ModeLabelNumbering = localization.T("massRename.modes.numbering");
        ModeLabelTemplate = localization.T("massRename.modes.template");
        StemOnlyLabel = localization.T("massRename.stemOnly");
        SortLabel = localization.T("massRename.sort");
        FindLabel = localization.T("massRename.find");
        ReplaceLabel = localization.T("massRename.replace");
        CaseSensitiveLabel = localization.T("massRename.caseSensitive");
        PrefixLabel = localization.T("massRename.prefix");
        SuffixLabel = localization.T("massRename.suffix");
        TemplateLabel = localization.T("massRename.template");
        StartLabel = localization.T("massRename.start");
        StepLabel = localization.T("massRename.step");
        PatternLabel = localization.T("massRename.pattern");
        ReplacementLabel = localization.T("massRename.replacement");
        FlagsLabel = localization.T("massRename.flags");
        OldNameLabel = localization.T("massRename.oldName");
        NewNameLabel = localization.T("massRename.newName");
        NoFilesLabel = localization.T("massRename.noFilesInScope");
        SortOptionLabels =
        [
            localization.T("massRename.sortPath"),
            localization.T("massRename.sortName"),
        ];
        NumberingPlaceholder = "{n:02} - {name}";
        TemplatePlaceholder = "{n:02} - {name}{ext}";

        Title = localization.T("massRename.title");
        ScopeDetail = localization.Format(
            "massRename.scopeDetail",
            ("label", MassRenameEngine.FormatScopeLabel(scopePath)),
            ("count", _scopeFiles.Count.ToString()));

        _debounce.Tick += (_, _) =>
        {
            _debounce.Stop();
            RebuildPreview();
        };

        RebuildPreview();
    }

    partial void OnSelectedModeIndexChanged(int value)
    {
        NotifyModePanels();
        UpdateHelp();
        SchedulePreview();
    }

    partial void OnStemOnlyChanged(bool value) => SchedulePreview();
    partial void OnSortChanged(MassRenameSort value) => SchedulePreview();

    partial void OnSortIndexChanged(int value)
    {
        var sort = value == 1 ? MassRenameSort.Name : MassRenameSort.Path;
        if (Sort != sort)
            Sort = sort;
    }
    partial void OnFindChanged(string value) => SchedulePreview();
    partial void OnReplaceChanged(string value) => SchedulePreview();
    partial void OnCaseSensitiveChanged(bool value) => SchedulePreview();
    partial void OnPrefixChanged(string value) => SchedulePreview();
    partial void OnSuffixChanged(string value) => SchedulePreview();
    partial void OnNumberingTemplateChanged(string value) => SchedulePreview();
    partial void OnNumberingStartChanged(int value) => SchedulePreview();
    partial void OnNumberingStepChanged(int value) => SchedulePreview();
    partial void OnRegexPatternChanged(string value) => SchedulePreview();
    partial void OnRegexReplacementChanged(string value) => SchedulePreview();
    partial void OnRegexFlagsChanged(string value) => SchedulePreview();
    partial void OnTemplateChanged(string value) => SchedulePreview();

    [RelayCommand]
    private void SelectMode(string? indexText)
    {
        if (!int.TryParse(indexText, out var index) || index < 0 || index >= ModeOrder.Length)
            return;

        SelectedModeIndex = index;
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        if (!CanApply || IsBusy)
            return;

        IsBusy = true;
        try
        {
            var operations = _plan
                .Where(entry => entry.Changed)
                .Select(entry => new TorrentRenameOperationDto(entry.Path, entry.NewName))
                .ToList();

            await _handlers.InvokeAsync(sp =>
                sp.GetRequiredService<ExecuteTorrentRenameBatchHandler>().HandleAsync(
                    _torrentId,
                    new TorrentRenameBatchRequestDto(operations)));

            Applied = true;
        }
        catch (Exception ex)
        {
            ValidationErrors.Clear();
            ValidationErrors.Add(ex.Message);
            CanApply = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SchedulePreview()
    {
        _debounce.Stop();
        _debounce.Start();
    }

    private void RebuildPreview()
    {
        var rule = BuildRule();
        _plan = MassRenameEngine.BuildRenamePlan(_scopeFiles, rule);
        var ruleErrors = MassRenameEngine.ValidateMassRenameRule(rule, _scopeFiles);
        var validation = MassRenameEngine.ValidatePlan(_plan);
        var errors = ruleErrors.Concat(validation.Errors).ToList();

        ValidationErrors.Clear();
        foreach (var error in errors)
            ValidationErrors.Add(error);

        ValidationWarnings.Clear();
        foreach (var warning in validation.Warnings)
            ValidationWarnings.Add(warning);

        var changedCount = _plan.Count(entry => entry.Changed);
        PreviewRows.Clear();
        foreach (var row in _plan.Take(PreviewRowLimit))
            PreviewRows.Add(row);

        var hidden = _plan.Count - PreviewRows.Count;
        PreviewLimitedText = hidden > 0
            ? _localization.Format("massRename.previewLimited", ("count", hidden.ToString()))
            : string.Empty;

        PreviewTitle = changedCount > 0
            ? $"{_localization.T("massRename.preview")}  {_localization.Format("massRename.changes", ("count", changedCount.ToString()))}"
            : _localization.T("massRename.preview");

        ApplyButtonText = _localization.Format("massRename.applyWithCount", ("count", changedCount.ToString()));
        CanApply = errors.Count == 0 && validation.CanApply;

        OnPropertyChanged(nameof(HasPreviewRows));
        OnPropertyChanged(nameof(ShowPreviewLimited));
        UpdateHelp();
        NotifyModePanels();
    }

    private void UpdateHelp()
    {
        var mode = ModeOrder[Math.Clamp(SelectedModeIndex, 0, ModeOrder.Length - 1)];
        var baseKey = $"massRename.help.{ModeKey(mode)}";

        GeneralSummary = _localization.T("massRename.general.summary");
        HelpTitle = _localization.T($"{baseKey}.title");
        HelpSummary = _localization.T($"{baseKey}.summary");
        GeneralStemSort =
            $"{_localization.T("massRename.general.stemOnly")} {_localization.T("massRename.general.sort")}";

        HelpExamples.Clear();
        foreach (var example in _localization.TList($"{baseKey}.examples"))
            HelpExamples.Add(example);
    }

    private void NotifyModePanels()
    {
        OnPropertyChanged(nameof(IsRegexMode));
        OnPropertyChanged(nameof(IsFindReplaceMode));
        OnPropertyChanged(nameof(IsPrefixSuffixMode));
        OnPropertyChanged(nameof(IsNumberingMode));
        OnPropertyChanged(nameof(IsTemplateMode));
    }

    private MassRenameRule BuildRule() =>
        new()
        {
            Mode = ModeOrder[Math.Clamp(SelectedModeIndex, 0, ModeOrder.Length - 1)],
            Find = Find,
            Replace = Replace,
            CaseSensitive = CaseSensitive,
            Prefix = Prefix,
            Suffix = Suffix,
            NumberingTemplate = NumberingTemplate,
            NumberingStart = NumberingStart,
            NumberingStep = NumberingStep,
            RegexPattern = RegexPattern,
            RegexReplacement = RegexReplacement,
            RegexFlags = RegexFlags,
            Template = Template,
            StemOnly = StemOnly,
            Sort = Sort,
        };

    private static string ModeKey(MassRenameMode mode) =>
        mode switch
        {
            MassRenameMode.Regex => "regex",
            MassRenameMode.FindReplace => "findReplace",
            MassRenameMode.PrefixSuffix => "prefixSuffix",
            MassRenameMode.Numbering => "numbering",
            MassRenameMode.Template => "template",
            _ => "regex",
        };
}
