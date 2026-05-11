namespace Tamp.CodeQL.V2;

/// <summary>Facade for the CodeQL 2.x CLI.</summary>
/// <remarks>
/// <para>Resolve via <c>[NuGetPackage(UseSystemPath = true)]</c>:</para>
/// <code>
/// [NuGetPackage("codeql", UseSystemPath = true)]
/// readonly Tool CodeQL;
/// </code>
/// </remarks>
public static class CodeQL
{
    /// <summary>Sub-facade for <c>codeql database &lt;verb&gt;</c>.</summary>
    public static class Database
    {
        public static CommandPlan Create(Tool tool, Action<CodeQLDatabaseCreateSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }

        public static CommandPlan Init(Tool tool, Action<CodeQLDatabaseInitSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }

        public static CommandPlan TraceCommand(Tool tool, Action<CodeQLDatabaseTraceCommandSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }

        public static CommandPlan Finalize(Tool tool, Action<CodeQLDatabaseFinalizeSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }

        public static CommandPlan Analyze(Tool tool, Action<CodeQLDatabaseAnalyzeSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }

        public static CommandPlan Upgrade(Tool tool, Action<CodeQLDatabaseUpgradeSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }

        public static CommandPlan ExportDiagnostics(Tool tool, Action<CodeQLDatabaseExportDiagnosticsSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }

        public static CommandPlan Bundle(Tool tool, Action<CodeQLDatabaseBundleSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }
    }

    /// <summary>Sub-facade for <c>codeql github &lt;verb&gt;</c>.</summary>
    public static class GitHub
    {
        public static CommandPlan UploadResults(Tool tool, Action<CodeQLGitHubUploadResultsSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }
    }

    /// <summary>Sub-facade for <c>codeql resolve &lt;verb&gt;</c>.</summary>
    public static class Resolve
    {
        public static CommandPlan Languages(Tool tool, Action<CodeQLResolveLanguagesSettings>? configure = null)
            => Build(tool, configure);

        public static CommandPlan Queries(Tool tool, Action<CodeQLResolveQueriesSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }
    }

    /// <summary>Sub-facade for <c>codeql pack &lt;verb&gt;</c>.</summary>
    public static class Pack
    {
        public static CommandPlan Download(Tool tool, Action<CodeQLPackDownloadSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }

        public static CommandPlan Install(Tool tool, Action<CodeQLPackInstallSettings>? configure = null)
            => Build(tool, configure);
    }

    /// <summary>Sub-facade for <c>codeql query &lt;verb&gt;</c>.</summary>
    public static class Query
    {
        public static CommandPlan Run(Tool tool, Action<CodeQLQueryRunSettings> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            return Build(tool, configure);
        }
    }

    /// <summary><c>codeql version</c></summary>
    public static CommandPlan Version(Tool tool, Action<CodeQLVersionSettings>? configure = null)
        => Build(tool, configure);

    /// <summary>Escape hatch for verbs we haven't typed.</summary>
    public static CommandPlan Raw(Tool tool, params string[] arguments)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        if (arguments is null || arguments.Length == 0)
            throw new ArgumentException("Raw requires at least one argument.", nameof(arguments));
        var s = new CodeQLRawSettings();
        s.AddArgs(arguments);
        return s.ToCommandPlan(tool);
    }

    private static CommandPlan Build<T>(Tool tool, Action<T>? configure) where T : CodeQLSettingsBase, new()
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var s = new T();
        configure?.Invoke(s);
        return s.ToCommandPlan(tool);
    }
}
