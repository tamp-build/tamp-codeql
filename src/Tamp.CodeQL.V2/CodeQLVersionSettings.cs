namespace Tamp.CodeQL.V2;

/// <summary>Settings for <c>codeql version</c>.</summary>
public sealed class CodeQLVersionSettings : CodeQLSettingsBase
{
    public string? Format { get; set; }

    public CodeQLVersionSettings SetFormat(string format) { Format = format; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        var args = new List<string> { "version" };
        if (!string.IsNullOrEmpty(Format)) { args.Add("--format"); args.Add(Format!); }
        return args;
    }
}
