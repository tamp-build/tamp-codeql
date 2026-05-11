namespace Tamp.CodeQL.V2;

/// <summary>
/// Common base for CodeQL 2.x verb settings. CodeQL has a uniform
/// option vocabulary across most verbs (--threads, --ram, --verbosity,
/// --logdir, etc.); concrete classes layer verb-specific args on top.
/// </summary>
public abstract class CodeQLSettingsBase
{
    public string? WorkingDirectory { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; } = new();

    /// <summary>Concurrent threads. Maps to <c>--threads</c> / <c>-j</c>. Use 0 for "all cores", negative for "leave N cores idle".</summary>
    public int? Threads { get; set; }

    /// <summary>RAM cap in MB. Maps to <c>--ram</c> / <c>-M</c>.</summary>
    public int? Ram { get; set; }

    /// <summary>Verbosity: <c>quiet</c>, <c>errors</c>, <c>warnings</c>, <c>progress</c>, <c>progress+</c>, <c>progress++</c>, <c>progress+++</c>. Maps to <c>--verbosity</c> / <c>-v</c>.</summary>
    public string? Verbosity { get; set; }

    /// <summary>Suppress progress output. Maps to <c>--quiet</c> / <c>-q</c>.</summary>
    public bool Quiet { get; set; }

    /// <summary>Disable the progress tracker (useful in non-interactive CI). Maps to <c>--no-progress-tracker</c>.</summary>
    public bool NoProgressTracker { get; set; }

    /// <summary>Write logs to a directory. Maps to <c>--logdir</c>.</summary>
    public string? LogDir { get; set; }

    /// <summary>Path to shared package/library caches. Maps to <c>--common-caches</c>.</summary>
    public string? CommonCaches { get; set; }

    /// <summary>Subclasses produce the verb tokens (e.g. <c>database create</c>) and verb-specific args.</summary>
    protected abstract IEnumerable<string> BuildVerbArguments();

    /// <summary>Subclasses override when they need stdin input (e.g. <c>github upload-results --github-auth-stdin</c>).</summary>
    protected virtual string? BuildStandardInput() => null;

    /// <summary>Subclasses override to register typed Secrets for redaction.</summary>
    protected virtual IReadOnlyList<Secret> CollectSecrets() => Array.Empty<Secret>();

    protected void EmitCommonArguments(List<string> args)
    {
        if (Threads is { } t) { args.Add("--threads"); args.Add(t.ToString()); }
        if (Ram is { } r) { args.Add("--ram"); args.Add(r.ToString()); }
        if (!string.IsNullOrEmpty(Verbosity)) { args.Add("--verbosity"); args.Add(Verbosity!); }
        if (Quiet) args.Add("--quiet");
        if (NoProgressTracker) args.Add("--no-progress-tracker");
        if (!string.IsNullOrEmpty(LogDir)) { args.Add("--logdir"); args.Add(LogDir!); }
        if (!string.IsNullOrEmpty(CommonCaches)) { args.Add("--common-caches"); args.Add(CommonCaches!); }
    }

    public CommandPlan ToCommandPlan(Tool tool)
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var args = BuildVerbArguments().ToList();
        return new CommandPlan
        {
            Executable = tool.Executable.Value,
            Arguments = args,
            Environment = new Dictionary<string, string>(EnvironmentVariables),
            WorkingDirectory = WorkingDirectory,
            StandardInput = BuildStandardInput(),
            Secrets = CollectSecrets(),
        };
    }
}

/// <summary>Generic fluent setters for the shared base.</summary>
public static class CodeQLSettingsBaseExtensions
{
    public static T SetWorkingDirectory<T>(this T s, string? cwd) where T : CodeQLSettingsBase { s.WorkingDirectory = cwd; return s; }
    public static T SetEnv<T>(this T s, string key, string value) where T : CodeQLSettingsBase { s.EnvironmentVariables[key] = value; return s; }
    public static T SetThreads<T>(this T s, int n) where T : CodeQLSettingsBase { s.Threads = n; return s; }
    public static T SetRam<T>(this T s, int megabytes) where T : CodeQLSettingsBase { s.Ram = megabytes; return s; }
    public static T SetVerbosity<T>(this T s, string level) where T : CodeQLSettingsBase { s.Verbosity = level; return s; }
    public static T SetQuiet<T>(this T s, bool v = true) where T : CodeQLSettingsBase { s.Quiet = v; return s; }
    public static T SetNoProgressTracker<T>(this T s, bool v = true) where T : CodeQLSettingsBase { s.NoProgressTracker = v; return s; }
    public static T SetLogDir<T>(this T s, string path) where T : CodeQLSettingsBase { s.LogDir = path; return s; }
    public static T SetCommonCaches<T>(this T s, string path) where T : CodeQLSettingsBase { s.CommonCaches = path; return s; }
}
