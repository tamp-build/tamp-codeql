using System.IO;
using Tamp;
using Xunit;

namespace Tamp.CodeQL.V2.Tests;

/// <summary>
/// Object-init overload coverage (TAM-161 satellite fanout). Every public
/// wrapper that accepts <c>Action&lt;TSettings&gt;</c> also exposes a
/// <c>(Tool, TSettings)</c> form; both shapes must produce identical
/// <see cref="CommandPlan"/>s.
/// </summary>
public sealed class ObjectInitTests
{
    private static Tool FakeTool(string name = "codeql") =>
        new(AbsolutePath.Create(Path.Combine(Path.GetTempPath(), name)));

    [Fact]
    public void DatabaseCreate_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();

        var fluent = CodeQL.Database.Create(tool, s => s
            .SetDatabasePath("dbs/main")
            .SetLanguage("csharp")
            .SetSourceRoot("src")
            .SetBuildMode("manual")
            .SetOverwrite()
            .SetExtractorOption("ms-build-runner", "dotnet"));

        var objectInit = CodeQL.Database.Create(tool, new CodeQLDatabaseCreateSettings
        {
            DatabasePath = "dbs/main",
            Language = "csharp",
            SourceRoot = "src",
            BuildMode = "manual",
            Overwrite = true,
            ExtractorOptions = { ["ms-build-runner"] = "dotnet" },
        });

        Assert.Equal(fluent.Executable, objectInit.Executable);
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void ObjectInit_Surface_Compiles_And_Returns_CommandPlan_For_Every_Wrapper()
    {
        // Smoke test: every wrapper accepts an object-init settings argument
        // and returns a non-null CommandPlan.
        var t = FakeTool();

        // Database sub-facade
        Assert.NotNull(CodeQL.Database.Create(t, new CodeQLDatabaseCreateSettings { DatabasePath = "db", Language = "csharp" }));
        Assert.NotNull(CodeQL.Database.Init(t, new CodeQLDatabaseInitSettings { DatabasePath = "db", Language = "csharp" }));
        Assert.NotNull(CodeQL.Database.TraceCommand(t, new CodeQLDatabaseTraceCommandSettings { DatabasePath = "db", BuildCommand = { "dotnet", "build" } }));
        Assert.NotNull(CodeQL.Database.Finalize(t, new CodeQLDatabaseFinalizeSettings { DatabasePath = "db" }));
        Assert.NotNull(CodeQL.Database.Analyze(t, new CodeQLDatabaseAnalyzeSettings { DatabasePath = "db" }));
        Assert.NotNull(CodeQL.Database.Upgrade(t, new CodeQLDatabaseUpgradeSettings { DatabasePath = "db" }));
        Assert.NotNull(CodeQL.Database.ExportDiagnostics(t, new CodeQLDatabaseExportDiagnosticsSettings { DatabasePath = "db" }));
        Assert.NotNull(CodeQL.Database.Bundle(t, new CodeQLDatabaseBundleSettings { DatabasePath = "db", Output = "out.zip" }));

        // GitHub sub-facade
        Assert.NotNull(CodeQL.GitHub.UploadResults(t, new CodeQLGitHubUploadResultsSettings
        {
            SarifFile = "results.sarif",
            Repository = "owner/repo",
            Ref = "refs/heads/main",
            Commit = "deadbeef",
        }));

        // Resolve sub-facade
        Assert.NotNull(CodeQL.Resolve.Languages(t, new CodeQLResolveLanguagesSettings()));
        Assert.NotNull(CodeQL.Resolve.Queries(t, new CodeQLResolveQueriesSettings { Targets = { "cs/security" } }));

        // Pack sub-facade
        Assert.NotNull(CodeQL.Pack.Download(t, new CodeQLPackDownloadSettings { Packs = { "codeql/csharp-queries" } }));
        Assert.NotNull(CodeQL.Pack.Install(t, new CodeQLPackInstallSettings { PackPath = "." }));

        // Query sub-facade
        Assert.NotNull(CodeQL.Query.Run(t, new CodeQLQueryRunSettings
        {
            QueryPath = "queries/foo.ql",
            Database = "db",
            Output = "out.bqrs",
        }));

        // Top-level
        Assert.NotNull(CodeQL.Version(t, new CodeQLVersionSettings { Format = "json" }));
    }
}
