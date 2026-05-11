namespace Tamp.CodeQL.V2;

/// <summary>Settings for <c>codeql query run</c> — run a single query against a database.</summary>
public sealed class CodeQLQueryRunSettings : CodeQLSettingsBase
{
    public string? QueryPath { get; set; }
    public string? Database { get; set; }
    public string? Output { get; set; }

    public CodeQLQueryRunSettings SetQueryPath(string path) { QueryPath = path; return this; }
    public CodeQLQueryRunSettings SetDatabase(string path) { Database = path; return this; }
    public CodeQLQueryRunSettings SetOutput(string path) { Output = path; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(QueryPath))
            throw new InvalidOperationException("codeql query run: QueryPath is required.");
        if (string.IsNullOrEmpty(Database))
            throw new InvalidOperationException("codeql query run: Database is required.");
        var args = new List<string> { "query", "run" };
        EmitCommonArguments(args);
        args.Add("--database"); args.Add(Database!);
        if (!string.IsNullOrEmpty(Output)) { args.Add("--output"); args.Add(Output!); }
        args.Add(QueryPath!);
        return args;
    }
}
