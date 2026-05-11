# Tamp.CodeQL

[CodeQL](https://github.com/github/codeql-cli-binaries) CLI wrapper
for [Tamp](https://github.com/tamp-build/tamp).

| Package | CodeQL | Status |
|---|---|---|
| [`Tamp.CodeQL.V2`](src/Tamp.CodeQL.V2) | 2.x | preview |

Requires `Tamp.Core ≥ 1.0.3`. GitHub PAT typed as `Secret` and fed
via `--github-auth-stdin` (NOT argv, so it can't leak to process
listings).

## Verbs

| Sub-facade | Verb | Notes |
|---|---|---|
| `Database` | `Create` | Index source. Required: `--language`, db path. `--source-root`, `--command`, `--build-mode`, `--overwrite`, `--extractor-option`. |
| `Database` | `Init` | Start build-tracing session. `--begin-tracing`. |
| `Database` | `TraceCommand` | Wrap a build command — wrapper emits `--` separator before the build argv. |
| `Database` | `Finalize` | Finalize traced DB. `--cleanup`. |
| `Database` | `Analyze` | Run queries. `--format`, `--output`, `--sarif-category`, `--sarif-add-snippets`, `--download` / `--no-download`. |
| `Database` | `Upgrade` | Schema upgrade. `--allow-downgrades`. |
| `Database` | `ExportDiagnostics` | Per-file diagnostics SARIF. |
| `Database` | `Bundle` | Package DB for upload. |
| `GitHub` | `UploadResults` | POST SARIF to Code Scanning. Token via Secret. |
| `Resolve` | `Languages` / `Queries` | Enumerate extractors / queries. |
| `Pack` | `Download` / `Install` | Fetch and install QL packs. |
| `Query` | `Run` | Single-query mode. |
| | `Version` | `codeql version`. |
| | `Raw` | Escape hatch. |

Common flags (all verbs): `--threads`, `--ram`, `--verbosity`,
`--quiet`, `--no-progress-tracker`, `--logdir`, `--common-caches`.

## Quick example — CI SARIF upload

```csharp
using Tamp;
using Tamp.CodeQL.V2;

[NuGetPackage("codeql", UseSystemPath = true)]
readonly Tool CodeQL = null!;

[Secret("GitHub token", EnvironmentVariable = "GITHUB_TOKEN")]
readonly Secret GitHubToken = null!;

Target CreateDb => _ => _.Executes(() =>
    CodeQL.Database.Create(CodeQL, s => s
        .SetDatabasePath("codeql-db")
        .SetLanguage("csharp")
        .SetSourceRoot(".")
        .SetCommand("dotnet build")
        .SetOverwrite()
        .SetRam(8000)
        .SetThreads(0)));

Target Analyze => _ => _
    .DependsOn(nameof(CreateDb))
    .Executes(() =>
        CodeQL.Database.Analyze(CodeQL, s => s
            .SetDatabasePath("codeql-db")
            .AddQuery("codeql/csharp-queries")
            .SetFormat("sarif-latest")
            .SetOutput("results.sarif")
            .SetSarifCategory("primary")
            .SetSarifAddSnippets()));

Target Upload => _ => _
    .DependsOn(nameof(Analyze))
    .Requires(() => GitHubToken != null)
    .Executes(() =>
        CodeQL.GitHub.UploadResults(CodeQL, s => s
            .SetSarifFile("results.sarif")
            .SetRepository("acme/widgets")
            .SetRef("refs/heads/main")
            .SetCommit(Git.Commit)
            .SetGitHubToken(GitHubToken)
            .SetWaitForProcessing()
            .SetWaitForProcessingTimeout(120)));
```

## Releasing

See [MAINTAINERS.md](MAINTAINERS.md).
