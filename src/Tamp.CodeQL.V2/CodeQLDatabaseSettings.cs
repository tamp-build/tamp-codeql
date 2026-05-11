namespace Tamp.CodeQL.V2;

/// <summary>
/// Settings for <c>codeql database create &lt;db-path&gt;</c> — index
/// source code into a CodeQL database. The compiled-language path
/// (Java/C#/C++/Go) requires <c>--command</c> or build tracing; the
/// non-compiled-language path (JS/Python/Ruby) uses
/// <c>--source-root</c> directly.
/// </summary>
public sealed class CodeQLDatabaseCreateSettings : CodeQLSettingsBase
{
    /// <summary>Database directory to create. Positional, required.</summary>
    public string? DatabasePath { get; set; }

    /// <summary>Source language (e.g. <c>csharp</c>, <c>javascript</c>). Maps to <c>--language</c> / <c>-l</c>.</summary>
    public string? Language { get; set; }

    /// <summary>Source root. Maps to <c>--source-root</c> / <c>-s</c>.</summary>
    public string? SourceRoot { get; set; }

    /// <summary>Build command (for compiled languages). Maps to <c>--command</c> / <c>-c</c>.</summary>
    public string? Command { get; set; }

    /// <summary>Build mode for languages that support it: <c>none</c>, <c>autobuild</c>, <c>manual</c>. Maps to <c>--build-mode</c>.</summary>
    public string? BuildMode { get; set; }

    /// <summary>Overwrite an existing database directory. Maps to <c>--overwrite</c>.</summary>
    public bool Overwrite { get; set; }

    /// <summary>Force overwrite even if target exists and looks unrelated. Maps to <c>--force-overwrite</c>.</summary>
    public bool ForceOverwrite { get; set; }

    /// <summary>Don't index the file contents (smaller DB). Maps to <c>--no-run-unnecessary-builds</c>.</summary>
    public bool NoRunUnnecessaryBuilds { get; set; }

    /// <summary>Extractor packs (advanced). Repeated as <c>--extractor-option</c>.</summary>
    public Dictionary<string, string> ExtractorOptions { get; } = new();

    public CodeQLDatabaseCreateSettings SetDatabasePath(string path) { DatabasePath = path; return this; }
    public CodeQLDatabaseCreateSettings SetLanguage(string lang) { Language = lang; return this; }
    public CodeQLDatabaseCreateSettings SetSourceRoot(string path) { SourceRoot = path; return this; }
    public CodeQLDatabaseCreateSettings SetCommand(string cmd) { Command = cmd; return this; }
    public CodeQLDatabaseCreateSettings SetBuildMode(string mode) { BuildMode = mode; return this; }
    public CodeQLDatabaseCreateSettings SetOverwrite(bool v = true) { Overwrite = v; return this; }
    public CodeQLDatabaseCreateSettings SetForceOverwrite(bool v = true) { ForceOverwrite = v; return this; }
    public CodeQLDatabaseCreateSettings SetNoRunUnnecessaryBuilds(bool v = true) { NoRunUnnecessaryBuilds = v; return this; }
    public CodeQLDatabaseCreateSettings SetExtractorOption(string key, string value) { ExtractorOptions[key] = value; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(DatabasePath))
            throw new InvalidOperationException("codeql database create: DatabasePath is required.");
        if (string.IsNullOrEmpty(Language))
            throw new InvalidOperationException("codeql database create: Language is required.");
        var args = new List<string> { "database", "create" };
        EmitCommonArguments(args);
        args.Add("--language"); args.Add(Language!);
        if (!string.IsNullOrEmpty(SourceRoot)) { args.Add("--source-root"); args.Add(SourceRoot!); }
        if (!string.IsNullOrEmpty(Command)) { args.Add("--command"); args.Add(Command!); }
        if (!string.IsNullOrEmpty(BuildMode)) { args.Add("--build-mode"); args.Add(BuildMode!); }
        if (Overwrite) args.Add("--overwrite");
        if (ForceOverwrite) args.Add("--force-overwrite");
        if (NoRunUnnecessaryBuilds) args.Add("--no-run-unnecessary-builds");
        foreach (var (k, v) in ExtractorOptions) { args.Add("--extractor-option"); args.Add($"{k}={v}"); }
        args.Add(DatabasePath!);
        return args;
    }
}

/// <summary>Settings for <c>codeql database init &lt;db-path&gt;</c> — start a build-tracing session.</summary>
public sealed class CodeQLDatabaseInitSettings : CodeQLSettingsBase
{
    public string? DatabasePath { get; set; }
    public string? Language { get; set; }
    public string? SourceRoot { get; set; }
    public string? BuildMode { get; set; }
    public bool Overwrite { get; set; }
    public bool ForceOverwrite { get; set; }
    /// <summary>Maps to <c>--begin-tracing</c>.</summary>
    public bool BeginTracing { get; set; }

    public CodeQLDatabaseInitSettings SetDatabasePath(string path) { DatabasePath = path; return this; }
    public CodeQLDatabaseInitSettings SetLanguage(string lang) { Language = lang; return this; }
    public CodeQLDatabaseInitSettings SetSourceRoot(string path) { SourceRoot = path; return this; }
    public CodeQLDatabaseInitSettings SetBuildMode(string mode) { BuildMode = mode; return this; }
    public CodeQLDatabaseInitSettings SetOverwrite(bool v = true) { Overwrite = v; return this; }
    public CodeQLDatabaseInitSettings SetForceOverwrite(bool v = true) { ForceOverwrite = v; return this; }
    public CodeQLDatabaseInitSettings SetBeginTracing(bool v = true) { BeginTracing = v; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(DatabasePath))
            throw new InvalidOperationException("codeql database init: DatabasePath is required.");
        if (string.IsNullOrEmpty(Language))
            throw new InvalidOperationException("codeql database init: Language is required.");
        var args = new List<string> { "database", "init" };
        EmitCommonArguments(args);
        args.Add("--language"); args.Add(Language!);
        if (!string.IsNullOrEmpty(SourceRoot)) { args.Add("--source-root"); args.Add(SourceRoot!); }
        if (!string.IsNullOrEmpty(BuildMode)) { args.Add("--build-mode"); args.Add(BuildMode!); }
        if (Overwrite) args.Add("--overwrite");
        if (ForceOverwrite) args.Add("--force-overwrite");
        if (BeginTracing) args.Add("--begin-tracing");
        args.Add(DatabasePath!);
        return args;
    }
}

/// <summary>Settings for <c>codeql database trace-command &lt;db-path&gt; -- &lt;build-cmd&gt;</c>.</summary>
public sealed class CodeQLDatabaseTraceCommandSettings : CodeQLSettingsBase
{
    public string? DatabasePath { get; set; }
    /// <summary>Build command + args to trace.</summary>
    public List<string> BuildCommand { get; } = [];

    public CodeQLDatabaseTraceCommandSettings SetDatabasePath(string path) { DatabasePath = path; return this; }
    public CodeQLDatabaseTraceCommandSettings AddCommandArg(string arg) { BuildCommand.Add(arg); return this; }
    public CodeQLDatabaseTraceCommandSettings SetCommand(params string[] argv) { BuildCommand.Clear(); BuildCommand.AddRange(argv); return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(DatabasePath))
            throw new InvalidOperationException("codeql database trace-command: DatabasePath is required.");
        if (BuildCommand.Count == 0)
            throw new InvalidOperationException("codeql database trace-command: BuildCommand is required.");
        var args = new List<string> { "database", "trace-command" };
        EmitCommonArguments(args);
        args.Add(DatabasePath!);
        args.Add("--");
        args.AddRange(BuildCommand);
        return args;
    }
}

/// <summary>Settings for <c>codeql database finalize &lt;db-path&gt;</c>.</summary>
public sealed class CodeQLDatabaseFinalizeSettings : CodeQLSettingsBase
{
    public string? DatabasePath { get; set; }
    public bool Cleanup { get; set; }

    public CodeQLDatabaseFinalizeSettings SetDatabasePath(string path) { DatabasePath = path; return this; }
    public CodeQLDatabaseFinalizeSettings SetCleanup(bool v = true) { Cleanup = v; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(DatabasePath))
            throw new InvalidOperationException("codeql database finalize: DatabasePath is required.");
        var args = new List<string> { "database", "finalize" };
        EmitCommonArguments(args);
        if (Cleanup) args.Add("--cleanup");
        args.Add(DatabasePath!);
        return args;
    }
}

/// <summary>Settings for <c>codeql database analyze &lt;db-path&gt; [suite-or-pack...]</c>.</summary>
public sealed class CodeQLDatabaseAnalyzeSettings : CodeQLSettingsBase
{
    public string? DatabasePath { get; set; }

    /// <summary>Query suites or pack references (positional, after the DB path).</summary>
    public List<string> Queries { get; } = [];

    /// <summary>Output format: <c>sarif-latest</c>, <c>sarifv2.1.0</c>, <c>csv</c>, <c>sarif-perdir</c>, <c>graphtext</c>, etc. Maps to <c>--format</c>.</summary>
    public string? Format { get; set; }

    /// <summary>Output file path. Maps to <c>--output</c> / <c>-o</c>.</summary>
    public string? Output { get; set; }

    /// <summary>SARIF category for the run. Maps to <c>--sarif-category</c>.</summary>
    public string? SarifCategory { get; set; }

    /// <summary>Add snippets to SARIF output. Maps to <c>--sarif-add-snippets</c>.</summary>
    public bool SarifAddSnippets { get; set; }

    /// <summary>Add baseline file paths counters. Maps to <c>--sarif-add-baseline-file-info</c>.</summary>
    public bool SarifAddBaselineFileInfo { get; set; }

    /// <summary>Add file context to SARIF. Maps to <c>--sarif-add-file-contents</c>.</summary>
    public bool SarifAddFileContents { get; set; }

    /// <summary>Don't fail on stale or unloadable packs. Maps to <c>--no-download</c>.</summary>
    public bool NoDownload { get; set; }

    /// <summary>Download missing packs before running. Maps to <c>--download</c>.</summary>
    public bool Download { get; set; }

    /// <summary>Rerun cached queries. Maps to <c>--rerun</c>.</summary>
    public bool Rerun { get; set; }

    /// <summary>Don't rerun queries that have cached results. Maps to <c>--no-rerun</c>.</summary>
    public bool NoRerun { get; set; }

    public CodeQLDatabaseAnalyzeSettings SetDatabasePath(string path) { DatabasePath = path; return this; }
    public CodeQLDatabaseAnalyzeSettings AddQuery(string suiteOrPack) { Queries.Add(suiteOrPack); return this; }
    public CodeQLDatabaseAnalyzeSettings SetFormat(string format) { Format = format; return this; }
    public CodeQLDatabaseAnalyzeSettings SetOutput(string path) { Output = path; return this; }
    public CodeQLDatabaseAnalyzeSettings SetSarifCategory(string category) { SarifCategory = category; return this; }
    public CodeQLDatabaseAnalyzeSettings SetSarifAddSnippets(bool v = true) { SarifAddSnippets = v; return this; }
    public CodeQLDatabaseAnalyzeSettings SetSarifAddBaselineFileInfo(bool v = true) { SarifAddBaselineFileInfo = v; return this; }
    public CodeQLDatabaseAnalyzeSettings SetSarifAddFileContents(bool v = true) { SarifAddFileContents = v; return this; }
    public CodeQLDatabaseAnalyzeSettings SetNoDownload(bool v = true) { NoDownload = v; return this; }
    public CodeQLDatabaseAnalyzeSettings SetDownload(bool v = true) { Download = v; return this; }
    public CodeQLDatabaseAnalyzeSettings SetRerun(bool v = true) { Rerun = v; return this; }
    public CodeQLDatabaseAnalyzeSettings SetNoRerun(bool v = true) { NoRerun = v; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(DatabasePath))
            throw new InvalidOperationException("codeql database analyze: DatabasePath is required.");
        var args = new List<string> { "database", "analyze" };
        EmitCommonArguments(args);
        if (!string.IsNullOrEmpty(Format)) { args.Add("--format"); args.Add(Format!); }
        if (!string.IsNullOrEmpty(Output)) { args.Add("--output"); args.Add(Output!); }
        if (!string.IsNullOrEmpty(SarifCategory)) { args.Add("--sarif-category"); args.Add(SarifCategory!); }
        if (SarifAddSnippets) args.Add("--sarif-add-snippets");
        if (SarifAddBaselineFileInfo) args.Add("--sarif-add-baseline-file-info");
        if (SarifAddFileContents) args.Add("--sarif-add-file-contents");
        if (NoDownload) args.Add("--no-download");
        if (Download) args.Add("--download");
        if (Rerun) args.Add("--rerun");
        if (NoRerun) args.Add("--no-rerun");
        args.Add(DatabasePath!);
        foreach (var q in Queries) args.Add(q);
        return args;
    }
}

/// <summary>Settings for <c>codeql database upgrade &lt;db-path&gt;</c>.</summary>
public sealed class CodeQLDatabaseUpgradeSettings : CodeQLSettingsBase
{
    public string? DatabasePath { get; set; }
    public bool AllowDowngrades { get; set; }

    public CodeQLDatabaseUpgradeSettings SetDatabasePath(string path) { DatabasePath = path; return this; }
    public CodeQLDatabaseUpgradeSettings SetAllowDowngrades(bool v = true) { AllowDowngrades = v; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(DatabasePath))
            throw new InvalidOperationException("codeql database upgrade: DatabasePath is required.");
        var args = new List<string> { "database", "upgrade" };
        EmitCommonArguments(args);
        if (AllowDowngrades) args.Add("--allow-downgrades");
        args.Add(DatabasePath!);
        return args;
    }
}

/// <summary>Settings for <c>codeql database export-diagnostics &lt;db-path&gt;</c>.</summary>
public sealed class CodeQLDatabaseExportDiagnosticsSettings : CodeQLSettingsBase
{
    public string? DatabasePath { get; set; }
    public string? Format { get; set; }
    public string? Output { get; set; }
    public string? SarifCategory { get; set; }

    public CodeQLDatabaseExportDiagnosticsSettings SetDatabasePath(string path) { DatabasePath = path; return this; }
    public CodeQLDatabaseExportDiagnosticsSettings SetFormat(string format) { Format = format; return this; }
    public CodeQLDatabaseExportDiagnosticsSettings SetOutput(string path) { Output = path; return this; }
    public CodeQLDatabaseExportDiagnosticsSettings SetSarifCategory(string category) { SarifCategory = category; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(DatabasePath))
            throw new InvalidOperationException("codeql database export-diagnostics: DatabasePath is required.");
        var args = new List<string> { "database", "export-diagnostics" };
        EmitCommonArguments(args);
        if (!string.IsNullOrEmpty(Format)) { args.Add("--format"); args.Add(Format!); }
        if (!string.IsNullOrEmpty(Output)) { args.Add("--output"); args.Add(Output!); }
        if (!string.IsNullOrEmpty(SarifCategory)) { args.Add("--sarif-category"); args.Add(SarifCategory!); }
        args.Add(DatabasePath!);
        return args;
    }
}

/// <summary>Settings for <c>codeql database bundle &lt;db-path&gt;</c> — package DB for upload/share.</summary>
public sealed class CodeQLDatabaseBundleSettings : CodeQLSettingsBase
{
    public string? DatabasePath { get; set; }
    public string? Output { get; set; }
    public bool IncludeUncompiledArtifacts { get; set; }

    public CodeQLDatabaseBundleSettings SetDatabasePath(string path) { DatabasePath = path; return this; }
    public CodeQLDatabaseBundleSettings SetOutput(string path) { Output = path; return this; }
    public CodeQLDatabaseBundleSettings SetIncludeUncompiledArtifacts(bool v = true) { IncludeUncompiledArtifacts = v; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(DatabasePath))
            throw new InvalidOperationException("codeql database bundle: DatabasePath is required.");
        if (string.IsNullOrEmpty(Output))
            throw new InvalidOperationException("codeql database bundle: Output is required.");
        var args = new List<string> { "database", "bundle" };
        EmitCommonArguments(args);
        args.Add("--output"); args.Add(Output!);
        if (IncludeUncompiledArtifacts) args.Add("--include-uncompiled-artifacts");
        args.Add(DatabasePath!);
        return args;
    }
}
