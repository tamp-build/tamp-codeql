using System.IO;
using Bogus;
using Tamp;
using Xunit;

namespace Tamp.CodeQL.V2.Tests;

public sealed class CodeQLTests
{
    private static Tool FakeTool(string name = "codeql") =>
        new(AbsolutePath.Create(Path.Combine(Path.GetTempPath(), name)));

    private static int IndexOf(IReadOnlyList<string> args, string value, int start = 0)
    {
        for (var i = start; i < args.Count; i++)
            if (args[i] == value) return i;
        return -1;
    }

    // ---- common shape ----

    [Fact]
    public void Every_Verb_Uses_Tool_Path()
    {
        var t = FakeTool();
        Assert.Equal(t.Executable.Value, CodeQL.Database.Create(t, s => s.SetDatabasePath("db").SetLanguage("csharp")).Executable);
        Assert.Equal(t.Executable.Value, CodeQL.Database.Analyze(t, s => s.SetDatabasePath("db")).Executable);
        Assert.Equal(t.Executable.Value, CodeQL.Resolve.Languages(t).Executable);
        Assert.Equal(t.Executable.Value, CodeQL.Pack.Install(t).Executable);
        Assert.Equal(t.Executable.Value, CodeQL.Version(t).Executable);
        Assert.Equal(t.Executable.Value, CodeQL.Raw(t, "--help").Executable);
    }

    // ---- database create ----

    [Fact]
    public void DatabaseCreate_Requires_Path_And_Language()
    {
        Assert.Throws<InvalidOperationException>(() => CodeQL.Database.Create(FakeTool(), s => { }));
        Assert.Throws<InvalidOperationException>(() => CodeQL.Database.Create(FakeTool(), s => s.SetDatabasePath("db")));
        Assert.Throws<InvalidOperationException>(() => CodeQL.Database.Create(FakeTool(), s => s.SetLanguage("csharp")));
    }

    [Fact]
    public void DatabaseCreate_Begins_With_database_create_And_Path_Trails()
    {
        var plan = CodeQL.Database.Create(FakeTool(), s => s
            .SetDatabasePath("dbs/main")
            .SetLanguage("csharp")
            .SetSourceRoot("src")
            .SetCommand("dotnet build")
            .SetBuildMode("manual")
            .SetOverwrite()
            .SetExtractorOption("ms-build-runner", "dotnet"));
        var args = plan.Arguments;
        Assert.Equal("database", args[0]);
        Assert.Equal("create", args[1]);
        Assert.Equal("dbs/main", args[^1]);
        Assert.Contains("--language", args); Assert.Contains("csharp", args);
        Assert.Contains("--source-root", args); Assert.Contains("src", args);
        Assert.Contains("--command", args); Assert.Contains("dotnet build", args);
        Assert.Contains("--build-mode", args); Assert.Contains("manual", args);
        Assert.Contains("--overwrite", args);
        Assert.Contains("--extractor-option", args); Assert.Contains("ms-build-runner=dotnet", args);
    }

    [Fact]
    public void DatabaseCreate_Common_Flags_Round_Trip()
    {
        var plan = CodeQL.Database.Create(FakeTool(), s => s
            .SetDatabasePath("db")
            .SetLanguage("javascript")
            .SetThreads(4)
            .SetRam(8000)
            .SetVerbosity("warnings")
            .SetQuiet()
            .SetNoProgressTracker()
            .SetLogDir("logs")
            .SetCommonCaches("/cache"));
        var args = plan.Arguments;
        Assert.Contains("--threads", args); Assert.Contains("4", args);
        Assert.Contains("--ram", args); Assert.Contains("8000", args);
        Assert.Contains("--verbosity", args); Assert.Contains("warnings", args);
        Assert.Contains("--quiet", args);
        Assert.Contains("--no-progress-tracker", args);
        Assert.Contains("--logdir", args); Assert.Contains("logs", args);
        Assert.Contains("--common-caches", args); Assert.Contains("/cache", args);
    }

    // ---- database init / trace-command / finalize ----

    [Fact]
    public void DatabaseInit_Round_Trips()
    {
        var plan = CodeQL.Database.Init(FakeTool(), s => s
            .SetDatabasePath("db")
            .SetLanguage("java")
            .SetSourceRoot(".")
            .SetBuildMode("none")
            .SetOverwrite()
            .SetBeginTracing());
        var args = plan.Arguments;
        Assert.Equal(["database", "init"], args.Take(2));
        Assert.Equal("db", args[^1]);
        Assert.Contains("--begin-tracing", args);
    }

    [Fact]
    public void DatabaseTraceCommand_Emits_DoubleDash_Before_BuildCommand()
    {
        var plan = CodeQL.Database.TraceCommand(FakeTool(), s => s
            .SetDatabasePath("db")
            .SetCommand("dotnet", "build", "--no-restore"));
        var args = plan.Arguments;
        var dashIdx = IndexOf(args, "--");
        var pathIdx = IndexOf(args, "db");
        Assert.True(pathIdx >= 0);
        Assert.True(dashIdx > pathIdx, "expected `--` to come after the db path");
        // Build command tokens tail the verb in order.
        Assert.Equal("dotnet", args[^3]);
        Assert.Equal("build", args[^2]);
        Assert.Equal("--no-restore", args[^1]);
    }

    [Fact]
    public void DatabaseTraceCommand_Requires_Build_Command()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CodeQL.Database.TraceCommand(FakeTool(), s => s.SetDatabasePath("db")));
    }

    [Fact]
    public void DatabaseFinalize_With_Cleanup()
    {
        var plan = CodeQL.Database.Finalize(FakeTool(), s => s.SetDatabasePath("db").SetCleanup());
        var args = plan.Arguments;
        Assert.Equal(["database", "finalize"], args.Take(2));
        Assert.Contains("--cleanup", args);
        Assert.Equal("db", args[^1]);
    }

    // ---- database analyze ----

    [Fact]
    public void DatabaseAnalyze_Requires_DatabasePath()
    {
        Assert.Throws<InvalidOperationException>(() => CodeQL.Database.Analyze(FakeTool(), s => { }));
    }

    [Fact]
    public void DatabaseAnalyze_Queries_Tail_The_DatabasePath()
    {
        var plan = CodeQL.Database.Analyze(FakeTool(), s => s
            .SetDatabasePath("db")
            .AddQuery("codeql/csharp-queries:Security/CWE-079")
            .AddQuery("codeql/csharp-queries:Security/CWE-089")
            .SetFormat("sarif-latest")
            .SetOutput("results.sarif")
            .SetSarifCategory("primary")
            .SetSarifAddSnippets()
            .SetDownload());
        var args = plan.Arguments;
        var dbIdx = IndexOf(args, "db");
        Assert.True(dbIdx > 1);
        Assert.Equal("codeql/csharp-queries:Security/CWE-079", args[dbIdx + 1]);
        Assert.Equal("codeql/csharp-queries:Security/CWE-089", args[dbIdx + 2]);
        Assert.Contains("--format", args); Assert.Contains("sarif-latest", args);
        Assert.Contains("--output", args); Assert.Contains("results.sarif", args);
        Assert.Contains("--sarif-category", args); Assert.Contains("primary", args);
        Assert.Contains("--sarif-add-snippets", args);
        Assert.Contains("--download", args);
    }

    [Fact]
    public void DatabaseAnalyze_No_Queries_Is_Valid_Uses_Defaults()
    {
        // codeql falls back to default suites when no positional queries
        // are given — the wrapper shouldn't pre-judge.
        var plan = CodeQL.Database.Analyze(FakeTool(), s => s.SetDatabasePath("db"));
        Assert.Equal("db", plan.Arguments[^1]);
    }

    // ---- database upgrade / export-diagnostics / bundle ----

    [Fact]
    public void DatabaseUpgrade_Round_Trips()
    {
        var plan = CodeQL.Database.Upgrade(FakeTool(), s => s.SetDatabasePath("db").SetAllowDowngrades());
        Assert.Equal(["database", "upgrade"], plan.Arguments.Take(2));
        Assert.Contains("--allow-downgrades", plan.Arguments);
        Assert.Equal("db", plan.Arguments[^1]);
    }

    [Fact]
    public void DatabaseExportDiagnostics_Round_Trips()
    {
        var plan = CodeQL.Database.ExportDiagnostics(FakeTool(), s => s
            .SetDatabasePath("db")
            .SetFormat("sarif-latest")
            .SetOutput("diagnostics.sarif"));
        Assert.Equal(["database", "export-diagnostics"], plan.Arguments.Take(2));
        Assert.Contains("--format", plan.Arguments);
        Assert.Contains("sarif-latest", plan.Arguments);
        Assert.Contains("--output", plan.Arguments);
        Assert.Contains("diagnostics.sarif", plan.Arguments);
        Assert.Equal("db", plan.Arguments[^1]);
    }

    [Fact]
    public void DatabaseBundle_Requires_Output()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CodeQL.Database.Bundle(FakeTool(), s => s.SetDatabasePath("db")));
    }

    [Fact]
    public void DatabaseBundle_Round_Trips()
    {
        var plan = CodeQL.Database.Bundle(FakeTool(), s => s
            .SetDatabasePath("db")
            .SetOutput("bundle.zip")
            .SetIncludeUncompiledArtifacts());
        Assert.Equal(["database", "bundle"], plan.Arguments.Take(2));
        Assert.Contains("--output", plan.Arguments);
        Assert.Contains("bundle.zip", plan.Arguments);
        Assert.Contains("--include-uncompiled-artifacts", plan.Arguments);
        Assert.Equal("db", plan.Arguments[^1]);
    }

    // ---- github upload-results ----

    [Fact]
    public void GitHubUploadResults_Requires_All_Four_Positionals()
    {
        Assert.Throws<InvalidOperationException>(() => CodeQL.GitHub.UploadResults(FakeTool(), s => { }));
        Assert.Throws<InvalidOperationException>(() =>
            CodeQL.GitHub.UploadResults(FakeTool(), s => s.SetSarifFile("r.sarif")));
        Assert.Throws<InvalidOperationException>(() =>
            CodeQL.GitHub.UploadResults(FakeTool(), s => s.SetSarifFile("r.sarif").SetRepository("o/r")));
        Assert.Throws<InvalidOperationException>(() =>
            CodeQL.GitHub.UploadResults(FakeTool(), s => s.SetSarifFile("r.sarif").SetRepository("o/r").SetRef("refs/heads/main")));
    }

    [Fact]
    public void GitHubUploadResults_Without_Token_Has_No_Stdin_No_Auth_Flag()
    {
        var plan = CodeQL.GitHub.UploadResults(FakeTool(), s => s
            .SetSarifFile("r.sarif")
            .SetRepository("acme/widgets")
            .SetRef("refs/heads/main")
            .SetCommit("abc123"));
        Assert.Null(plan.StandardInput);
        Assert.Empty(plan.Secrets);
        Assert.DoesNotContain("--github-auth-stdin", plan.Arguments);
    }

    [Fact]
    public void GitHubUploadResults_With_Token_Feeds_Stdin_And_Registers_Secret()
    {
        var token = new Secret("GitHub PAT", "ghp_test_value_1234567890");
        var plan = CodeQL.GitHub.UploadResults(FakeTool(), s => s
            .SetSarifFile("r.sarif")
            .SetRepository("acme/widgets")
            .SetRef("refs/heads/main")
            .SetCommit("abc123")
            .SetGitHubToken(token));
        Assert.Contains("--github-auth-stdin", plan.Arguments);
        Assert.Equal("ghp_test_value_1234567890", plan.StandardInput);
        Assert.Single(plan.Secrets);
        Assert.Same(token, plan.Secrets[0]);
        // Token MUST NOT appear as an argv token (it would leak via process listings).
        Assert.DoesNotContain("ghp_test_value_1234567890", plan.Arguments);
    }

    [Fact]
    public void GitHubUploadResults_All_Flags_Round_Trip()
    {
        var plan = CodeQL.GitHub.UploadResults(FakeTool(), s => s
            .SetSarifFile("results.sarif")
            .SetRepository("acme/widgets")
            .SetRef("refs/pull/42/merge")
            .SetCommit("deadbeefcafe")
            .SetGitHubUrl("https://ghe.acme.com")
            .SetSarifCategory("e2e")
            .SetWaitForProcessing()
            .SetWaitForProcessingTimeout(120));
        var args = plan.Arguments;
        Assert.Equal(["github", "upload-results"], args.Take(2));
        Assert.Contains("--sarif", args); Assert.Contains("results.sarif", args);
        Assert.Contains("--repository", args); Assert.Contains("acme/widgets", args);
        Assert.Contains("--ref", args); Assert.Contains("refs/pull/42/merge", args);
        Assert.Contains("--commit", args); Assert.Contains("deadbeefcafe", args);
        Assert.Contains("--github-url", args); Assert.Contains("https://ghe.acme.com", args);
        Assert.Contains("--sarif-category", args); Assert.Contains("e2e", args);
        Assert.Contains("--wait-for-processing", args);
        Assert.Contains("--wait-for-processing-timeout", args); Assert.Contains("120", args);
    }

    // ---- resolve ----

    [Fact]
    public void ResolveLanguages_Default_Is_The_Verb()
    {
        Assert.Equal(["resolve", "languages"], CodeQL.Resolve.Languages(FakeTool()).Arguments);
    }

    [Fact]
    public void ResolveLanguages_With_Format()
    {
        var plan = CodeQL.Resolve.Languages(FakeTool(), s => s.SetFormat("json"));
        Assert.Contains("--format", plan.Arguments);
        Assert.Contains("json", plan.Arguments);
    }

    [Fact]
    public void ResolveQueries_Requires_At_Least_One_Target()
    {
        Assert.Throws<InvalidOperationException>(() => CodeQL.Resolve.Queries(FakeTool(), s => { }));
    }

    [Fact]
    public void ResolveQueries_Round_Trips()
    {
        var plan = CodeQL.Resolve.Queries(FakeTool(), s => s
            .AddSearchPath("/qlpacks")
            .SetFormat("json")
            .AddTarget("codeql/csharp-queries"));
        var args = plan.Arguments;
        Assert.Equal(["resolve", "queries"], args.Take(2));
        Assert.Contains("--search-path", args);
        Assert.Contains("/qlpacks", args);
        Assert.Equal("codeql/csharp-queries", args[^1]);
    }

    // ---- pack ----

    [Fact]
    public void PackDownload_Requires_At_Least_One_Pack()
    {
        Assert.Throws<InvalidOperationException>(() => CodeQL.Pack.Download(FakeTool(), s => { }));
    }

    [Fact]
    public void PackDownload_Multiple_Packs_Round_Trip()
    {
        var plan = CodeQL.Pack.Download(FakeTool(), s => s
            .AddPack("codeql/csharp-queries@1.0.0")
            .AddPack("codeql/java-queries@1.0.0")
            .SetDir("/cache/qlpacks"));
        var args = plan.Arguments;
        Assert.Equal(["pack", "download"], args.Take(2));
        Assert.Contains("--dir", args);
        Assert.Equal("codeql/csharp-queries@1.0.0", args[^2]);
        Assert.Equal("codeql/java-queries@1.0.0", args[^1]);
    }

    [Fact]
    public void PackInstall_With_Path_Round_Trips()
    {
        var plan = CodeQL.Pack.Install(FakeTool(), s => s.SetPackPath("./qlpacks/my").SetUseLockFile());
        var args = plan.Arguments;
        Assert.Equal(["pack", "install"], args.Take(2));
        Assert.Contains("--mode", args);
        Assert.Contains("use-lock", args);
        Assert.Equal("./qlpacks/my", args[^1]);
    }

    // ---- query run ----

    [Fact]
    public void QueryRun_Requires_QueryPath_And_Database()
    {
        Assert.Throws<InvalidOperationException>(() => CodeQL.Query.Run(FakeTool(), s => { }));
        Assert.Throws<InvalidOperationException>(() =>
            CodeQL.Query.Run(FakeTool(), s => s.SetQueryPath("q.ql")));
        Assert.Throws<InvalidOperationException>(() =>
            CodeQL.Query.Run(FakeTool(), s => s.SetDatabase("db")));
    }

    [Fact]
    public void QueryRun_Round_Trips()
    {
        var plan = CodeQL.Query.Run(FakeTool(), s => s
            .SetQueryPath("q.ql")
            .SetDatabase("db")
            .SetOutput("q.bqrs"));
        var args = plan.Arguments;
        Assert.Equal(["query", "run"], args.Take(2));
        Assert.Contains("--database", args);
        Assert.Contains("db", args);
        Assert.Contains("--output", args);
        Assert.Contains("q.bqrs", args);
        Assert.Equal("q.ql", args[^1]);
    }

    // ---- version ----

    [Fact]
    public void Version_Default_Is_Just_The_Verb()
    {
        Assert.Equal(["version"], CodeQL.Version(FakeTool()).Arguments);
    }

    [Fact]
    public void Version_With_Format()
    {
        var plan = CodeQL.Version(FakeTool(), s => s.SetFormat("json"));
        Assert.Equal(["version", "--format", "json"], plan.Arguments);
    }

    // ---- raw ----

    [Fact]
    public void Raw_Requires_Args()
    {
        Assert.Throws<ArgumentException>(() => CodeQL.Raw(FakeTool()));
    }

    [Fact]
    public void Raw_Forwards_Verbatim()
    {
        var plan = CodeQL.Raw(FakeTool(), "bqrs", "decode", "results.bqrs");
        Assert.Equal(["bqrs", "decode", "results.bqrs"], plan.Arguments);
    }

    // ---- nulls ----

    [Fact]
    public void Null_Tool_Throws_For_All_Verbs()
    {
        Assert.Throws<ArgumentNullException>(() => CodeQL.Database.Create(null!, s => s.SetDatabasePath("db").SetLanguage("cs")));
        Assert.Throws<ArgumentNullException>(() => CodeQL.Database.Analyze(null!, s => s.SetDatabasePath("db")));
        Assert.Throws<ArgumentNullException>(() => CodeQL.GitHub.UploadResults(null!, s => s.SetSarifFile("r").SetRepository("o/r").SetRef("x").SetCommit("y")));
        Assert.Throws<ArgumentNullException>(() => CodeQL.Resolve.Languages(null!));
        Assert.Throws<ArgumentNullException>(() => CodeQL.Pack.Install(null!));
        Assert.Throws<ArgumentNullException>(() => CodeQL.Version(null!));
        Assert.Throws<ArgumentNullException>(() => CodeQL.Raw(null!, "--help"));
    }

    [Fact]
    public void Null_Configurer_Throws_For_Required_Verbs()
    {
        Assert.Throws<ArgumentNullException>(() => CodeQL.Database.Create(FakeTool(), null!));
        Assert.Throws<ArgumentNullException>(() => CodeQL.Database.Analyze(FakeTool(), null!));
        Assert.Throws<ArgumentNullException>(() => CodeQL.GitHub.UploadResults(FakeTool(), null!));
        Assert.Throws<ArgumentNullException>(() => CodeQL.Resolve.Queries(FakeTool(), null!));
        Assert.Throws<ArgumentNullException>(() => CodeQL.Pack.Download(FakeTool(), null!));
        Assert.Throws<ArgumentNullException>(() => CodeQL.Query.Run(FakeTool(), null!));
    }

    [Fact]
    public void Working_Directory_And_Env_Flow_To_Plan()
    {
        var cwd = Path.GetTempPath();
        var plan = CodeQL.Database.Create(FakeTool(), s => s
            .SetDatabasePath("db")
            .SetLanguage("csharp")
            .SetWorkingDirectory(cwd)
            .SetEnv("CODEQL_RAM", "8000"));
        Assert.Equal(cwd, plan.WorkingDirectory);
        Assert.Equal("8000", plan.Environment["CODEQL_RAM"]);
    }

    [Fact]
    public void Many_Queries_Preserve_Order_Under_Random_Names()
    {
        // Analyze's positional query list order is observable (results
        // are reported per-query in argument order).
        var faker = new Faker();
        var queries = Enumerable.Range(0, 5).Select(_ => faker.Random.AlphaNumeric(10)).ToArray();
        var plan = CodeQL.Database.Analyze(FakeTool(), s =>
        {
            s.SetDatabasePath("db");
            foreach (var q in queries) s.AddQuery(q);
        });
        // The queries tail the verb after the db path.
        var dbIdx = IndexOf(plan.Arguments, "db");
        var observed = plan.Arguments.Skip(dbIdx + 1).ToArray();
        Assert.Equal(queries, observed);
    }
}
