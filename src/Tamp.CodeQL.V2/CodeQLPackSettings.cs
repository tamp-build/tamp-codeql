namespace Tamp.CodeQL.V2;

/// <summary>Settings for <c>codeql pack download</c> — fetch query packs from the CodeQL registry.</summary>
public sealed class CodeQLPackDownloadSettings : CodeQLSettingsBase
{
    /// <summary>Packs to download, e.g. <c>codeql/csharp-queries@1.0.0</c>. At least one required.</summary>
    public List<string> Packs { get; } = [];
    public string? Dir { get; set; }

    public CodeQLPackDownloadSettings AddPack(string spec) { Packs.Add(spec); return this; }
    public CodeQLPackDownloadSettings SetDir(string path) { Dir = path; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (Packs.Count == 0)
            throw new InvalidOperationException("codeql pack download: at least one pack reference is required.");
        var args = new List<string> { "pack", "download" };
        EmitCommonArguments(args);
        if (!string.IsNullOrEmpty(Dir)) { args.Add("--dir"); args.Add(Dir!); }
        foreach (var p in Packs) args.Add(p);
        return args;
    }
}

/// <summary>Settings for <c>codeql pack install [path]</c> — resolve dependencies of a pack.</summary>
public sealed class CodeQLPackInstallSettings : CodeQLSettingsBase
{
    public string? PackPath { get; set; }
    public bool Mode_UseLockFile { get; set; }

    public CodeQLPackInstallSettings SetPackPath(string path) { PackPath = path; return this; }
    public CodeQLPackInstallSettings SetUseLockFile(bool v = true) { Mode_UseLockFile = v; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "pack", "install" };
        EmitCommonArguments(args);
        if (Mode_UseLockFile) { args.Add("--mode"); args.Add("use-lock"); }
        if (!string.IsNullOrEmpty(PackPath)) args.Add(PackPath!);
        return args;
    }
}
