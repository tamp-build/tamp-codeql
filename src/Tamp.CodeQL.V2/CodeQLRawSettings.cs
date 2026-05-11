namespace Tamp.CodeQL.V2;

/// <summary>Escape hatch for verbs we haven't typed (bqrs, test, generate, dataset, etc.).</summary>
public sealed class CodeQLRawSettings : CodeQLSettingsBase
{
    public List<string> RawArguments { get; } = [];

    public CodeQLRawSettings AddArgs(params string[] args) { RawArguments.AddRange(args); return this; }

    protected override IEnumerable<string> BuildVerbArguments() => RawArguments;
}
