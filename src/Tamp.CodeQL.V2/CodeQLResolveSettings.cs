namespace Tamp.CodeQL.V2;

/// <summary>Settings for <c>codeql resolve languages</c> — list available extractors.</summary>
public sealed class CodeQLResolveLanguagesSettings : CodeQLSettingsBase
{
    public string? Format { get; set; }

    public CodeQLResolveLanguagesSettings SetFormat(string format) { Format = format; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "resolve", "languages" };
        EmitCommonArguments(args);
        if (!string.IsNullOrEmpty(Format)) { args.Add("--format"); args.Add(Format!); }
        return args;
    }
}

/// <summary>Settings for <c>codeql resolve queries</c> — enumerate queries inside a pack or suite.</summary>
public sealed class CodeQLResolveQueriesSettings : CodeQLSettingsBase
{
    public List<string> SearchPaths { get; } = [];
    public string? Format { get; set; }
    public List<string> Targets { get; } = [];

    public CodeQLResolveQueriesSettings AddSearchPath(string path) { SearchPaths.Add(path); return this; }
    public CodeQLResolveQueriesSettings SetFormat(string format) { Format = format; return this; }
    public CodeQLResolveQueriesSettings AddTarget(string target) { Targets.Add(target); return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (Targets.Count == 0)
            throw new InvalidOperationException("codeql resolve queries: at least one target is required.");
        var args = new List<string> { "resolve", "queries" };
        EmitCommonArguments(args);
        foreach (var p in SearchPaths) { args.Add("--search-path"); args.Add(p); }
        if (!string.IsNullOrEmpty(Format)) { args.Add("--format"); args.Add(Format!); }
        foreach (var t in Targets) args.Add(t);
        return args;
    }
}
