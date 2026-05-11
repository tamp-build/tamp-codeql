namespace Tamp.CodeQL.V2;

/// <summary>
/// Settings for <c>codeql github upload-results</c> — POST a SARIF
/// file to GitHub's Code Scanning API. The auth token is read from
/// stdin via <c>--github-auth-stdin</c>, so we type it as a Secret
/// and feed it through the CommandPlan's StandardInput rather than
/// emitting it as an argv token (where it could leak into process
/// listings).
/// </summary>
public sealed class CodeQLGitHubUploadResultsSettings : CodeQLSettingsBase
{
    /// <summary>SARIF file to upload. Maps to <c>--sarif</c>. Required.</summary>
    public string? SarifFile { get; set; }

    /// <summary>Repository as <c>owner/name</c>. Maps to <c>--repository</c> / <c>-r</c>. Required.</summary>
    public string? Repository { get; set; }

    /// <summary>Ref like <c>refs/heads/main</c> or <c>refs/pull/123/merge</c>. Maps to <c>--ref</c>. Required.</summary>
    public string? Ref { get; set; }

    /// <summary>Commit SHA. Maps to <c>--commit</c> / <c>-c</c>. Required.</summary>
    public string? Commit { get; set; }

    /// <summary>GitHub PAT or GITHUB_TOKEN. Sent via <c>--github-auth-stdin</c> so it doesn't appear in argv.</summary>
    public Secret? GitHubToken { get; set; }

    /// <summary>Enterprise GitHub URL. Maps to <c>--github-url</c>.</summary>
    public string? GitHubUrl { get; set; }

    /// <summary>SARIF category (groups results in the UI). Maps to <c>--sarif-category</c>.</summary>
    public string? SarifCategory { get; set; }

    /// <summary>Wait until the upload is processed. Maps to <c>--wait-for-processing</c>.</summary>
    public bool WaitForProcessing { get; set; }

    /// <summary>Timeout (seconds) for <c>--wait-for-processing</c>. Maps to <c>--wait-for-processing-timeout</c>.</summary>
    public int? WaitForProcessingTimeout { get; set; }

    public CodeQLGitHubUploadResultsSettings SetSarifFile(string path) { SarifFile = path; return this; }
    public CodeQLGitHubUploadResultsSettings SetRepository(string ownerSlashName) { Repository = ownerSlashName; return this; }
    public CodeQLGitHubUploadResultsSettings SetRef(string @ref) { Ref = @ref; return this; }
    public CodeQLGitHubUploadResultsSettings SetCommit(string sha) { Commit = sha; return this; }
    public CodeQLGitHubUploadResultsSettings SetGitHubToken(Secret? token) { GitHubToken = token; return this; }
    public CodeQLGitHubUploadResultsSettings SetGitHubUrl(string url) { GitHubUrl = url; return this; }
    public CodeQLGitHubUploadResultsSettings SetSarifCategory(string category) { SarifCategory = category; return this; }
    public CodeQLGitHubUploadResultsSettings SetWaitForProcessing(bool v = true) { WaitForProcessing = v; return this; }
    public CodeQLGitHubUploadResultsSettings SetWaitForProcessingTimeout(int seconds) { WaitForProcessingTimeout = seconds; return this; }

    protected override IEnumerable<string> BuildVerbArguments()
    {
        if (string.IsNullOrEmpty(SarifFile))
            throw new InvalidOperationException("codeql github upload-results: SarifFile is required.");
        if (string.IsNullOrEmpty(Repository))
            throw new InvalidOperationException("codeql github upload-results: Repository is required.");
        if (string.IsNullOrEmpty(Ref))
            throw new InvalidOperationException("codeql github upload-results: Ref is required.");
        if (string.IsNullOrEmpty(Commit))
            throw new InvalidOperationException("codeql github upload-results: Commit is required.");
        var args = new List<string> { "github", "upload-results" };
        EmitCommonArguments(args);
        args.Add("--sarif"); args.Add(SarifFile!);
        args.Add("--repository"); args.Add(Repository!);
        args.Add("--ref"); args.Add(Ref!);
        args.Add("--commit"); args.Add(Commit!);
        if (!string.IsNullOrEmpty(GitHubUrl)) { args.Add("--github-url"); args.Add(GitHubUrl!); }
        if (!string.IsNullOrEmpty(SarifCategory)) { args.Add("--sarif-category"); args.Add(SarifCategory!); }
        if (WaitForProcessing) args.Add("--wait-for-processing");
        if (WaitForProcessingTimeout is { } t) { args.Add("--wait-for-processing-timeout"); args.Add(t.ToString()); }
        if (GitHubToken is not null) args.Add("--github-auth-stdin");
        return args;
    }

    protected override string? BuildStandardInput() => GitHubToken?.Reveal();

    protected override IReadOnlyList<Secret> CollectSecrets() =>
        GitHubToken is null ? Array.Empty<Secret>() : new[] { GitHubToken };
}
