using System.IO;
using Tamp;
using Xunit;
using Xunit.Abstractions;

namespace Tamp.CodeQL.V2.IntegrationTests;

/// <summary>
/// Exercises the wrapper against a real CodeQL 2.x CLI. We stick to
/// fast, network-free verbs: <c>version</c>, <c>resolve languages</c>,
/// and <c>--help</c> shape probes for the major verbs. Full <c>database
/// create</c> + <c>analyze</c> would require the codeql-bundle (~500MB)
/// + a real source repo to index, which doesn't pay for itself in
/// wrapper-correctness tests.
/// </summary>
public sealed class CodeQLIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public CodeQLIntegrationTests(ITestOutputHelper output) => _output = output;

    private static string? ResolveOnPath(string baseName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        var names = OperatingSystem.IsWindows()
            ? new[] { $"{baseName}.exe", $"{baseName}.cmd", $"{baseName}.bat", baseName }
            : new[] { baseName };
        foreach (var dir in pathEnv.Split(Path.PathSeparator))
        {
            if (string.IsNullOrEmpty(dir)) continue;
            foreach (var n in names)
            {
                var c = Path.Combine(dir, n);
                if (File.Exists(c)) return c;
            }
        }
        return null;
    }

    private static Tool ResolveTool() =>
        new(AbsolutePath.Create(ResolveOnPath("codeql")
            ?? throw new InvalidOperationException("codeql not found on PATH. Install: https://github.com/github/codeql-cli-binaries/releases")));

    private CaptureResult Run(CommandPlan plan)
    {
        _output.WriteLine($"$ {plan.Executable} {string.Join(' ', plan.Arguments)}");
        var result = ProcessRunner.Capture(plan);
        foreach (var line in result.Lines)
            _output.WriteLine($"  [{line.Type}] {line.Text}");
        _output.WriteLine($"  → exit {result.ExitCode}");
        return result;
    }

    [Fact]
    public void Version_Reports_2_x()
    {
        var tool = ResolveTool();
        var plan = CodeQL.Version(tool);
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        Assert.Matches(@"2\.\d+\.\d+", combined);
    }

    [Fact]
    public void Version_Json_Is_Parseable()
    {
        var tool = ResolveTool();
        var plan = CodeQL.Version(tool, s => s.SetFormat("json"));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        using var doc = System.Text.Json.JsonDocument.Parse(result.StdoutText);
        Assert.Equal(System.Text.Json.JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("version", out _),
            "Expected `version` key in `codeql version --format=json` output.");
    }

    [Fact]
    public void ResolveLanguages_Lists_Common_Extractors()
    {
        var tool = ResolveTool();
        var plan = CodeQL.Resolve.Languages(tool);
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        // Every CodeQL CLI ships these extractors at minimum.
        Assert.Contains("javascript", combined, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("python", combined, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Matches a flag name in CodeQL's help output. CodeQL renders
    /// booleans as <c>--[no-]flag</c>, mandatory args as
    /// <c>--flag=&lt;value&gt;</c>, optional args as <c>--flag &lt;value&gt;</c>,
    /// and short forms as <c>-x, --flag</c>. The regex accepts the
    /// flag with an optional <c>[no-]</c> prefix.
    /// </summary>
    private static bool HasFlag(string output, string flagName)
    {
        var bare = flagName.TrimStart('-');
        var pattern = $@"--(\[no-\])?{System.Text.RegularExpressions.Regex.Escape(bare)}\b";
        return System.Text.RegularExpressions.Regex.IsMatch(output, pattern);
    }

    [Fact]
    public void Raw_DatabaseAnalyze_Help_Surfaces_Expected_Flags()
    {
        // `codeql database analyze --help -v` prints the verb's full
        // flag list. CodeQL renders booleans as `--[no-]flag`, so we
        // match flag names through a regex that tolerates that prefix.
        var tool = ResolveTool();
        var plan = CodeQL.Raw(tool, "database", "analyze", "--help", "-v");
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        foreach (var flag in new[] { "--format", "--output", "--sarif-category", "--sarif-add-snippets", "--threads", "--ram" })
        {
            Assert.True(HasFlag(combined, flag), $"Expected flag {flag} in `analyze --help -v` output.");
        }
    }

    [Fact]
    public void Raw_DatabaseCreate_Help_Surfaces_Expected_Flags()
    {
        var tool = ResolveTool();
        var plan = CodeQL.Raw(tool, "database", "create", "--help", "-v");
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        foreach (var flag in new[] { "--language", "--source-root", "--command", "--overwrite" })
        {
            Assert.True(HasFlag(combined, flag), $"Expected flag {flag} in `create --help -v` output.");
        }
    }

    [Fact]
    public void Raw_GitHubUploadResults_Help_Surfaces_Expected_Flags()
    {
        var tool = ResolveTool();
        var plan = CodeQL.Raw(tool, "github", "upload-results", "--help", "-v");
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var combined = result.StdoutText + result.StderrText;
        foreach (var flag in new[] { "--sarif", "--repository", "--ref", "--commit", "--github-auth-stdin" })
        {
            Assert.True(HasFlag(combined, flag), $"Expected flag {flag} in `upload-results --help -v` output.");
        }
    }
}
