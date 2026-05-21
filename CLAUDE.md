# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build, Test, Run

The solution file is `SqlServer.Rules.slnx` (slnx format — handled by modern `dotnet` SDK). CI uses .NET 10 SDK; the rules library targets `netstandard2.1` + `net472`, tools/tests target `net8.0` and `net10.0`.

```pwsh
dotnet restore
dotnet build --no-restore --configuration Release
dotnet test --no-build --configuration Release
```

Run a single test project / single test:

```pwsh
dotnet test test/SqlServer.Rules.Test/SqlServer.Rules.Tests.csproj
dotnet test test/SqlServer.Rules.Test/SqlServer.Rules.Tests.csproj --filter "FullyQualifiedName~SRD0039Tests"
```

Run the CLI against samples after a build:

```pwsh
./tools/SqlAnalyzerCli/bin/Release/net10.0/ErikEJ.TSQLAnalyzerCli -i tools/SqlAnalyzerCli/testfiles/simple.sql
./tools/SqlAnalyzerCli/bin/Release/net10.0/ErikEJ.TSQLAnalyzerCli -i tools/SqlAnalyzerCli/testfiles/Chinook.dacpac
```

The VSIX (`tools/SqlAnalyzerVsix`) is **Windows-only** (WPF) and is excluded from the main build (`<Build Project="false" />` in the slnx). Build it with MSBuild directly when needed.

`sqlprojects/*` are also marked non-build in the slnx — they exist as test fixtures (real SQL projects with intentional rule violations) consumed by the test projects via relative file paths.

## Architecture

### How a rule works

1. `src/SqlServer.Rules/` is packaged as `ErikEJ.DacFX.SqlServer.Rules` — a DacFx code-analysis NuGet that drops `SqlServer.Rules.dll` / `SqlServer.Rules.NetFx.dll` into `analyzers/dotnet/cs/` (the `net472` build is renamed to `.NetFx.dll` in a post-build step, see `src/SqlServer.Rules/SqlServer.Rules.csproj`). Both TFMs ship in one package so the analyzer loads under both legacy SSDT (net472) and modern SDK-style SQL projects.
2. Every rule inherits from `BaseSqlCodeAnalysisRule` (`src/SqlServer.Rules/BaseSqlCodeAnalysisRule.cs`), which extends DacFx's `SqlCodeAnalysisRule`. The base class exposes the shared `ProgrammingSchemas` / `ProgrammingAndViewSchemas` element-type sets, keyword/data-type lists, a shared case-insensitive `Comparer`, and the heavy `GetDataType(...)` / `GetColumnDataType(...)` helpers (with `ConditionalWeakTable` caches keyed on `TSqlObject` — don't bypass these caches, column-type resolution is expensive).
3. Rules are discovered by MEF via `[ExportCodeAnalysisRule]`. The `RuleId` is always `Constants.RuleNameSpace + "SR{D|N|P}####"` (e.g. `SqlServer.Rules.SRD0038`). The category comes from `Constants.Design` / `Naming` / `Performance` in `src/SqlServer.Rules/Globals/Constants.cs`. Rules are organized in folders by category that mirror these constants.
4. Most rules walk the ScriptDom AST using visitors under `src/SqlServer.Rules/Visitors/`. Prefer reusing an existing visitor — there are ~90 of them covering most node types. New visitors derive from `BaseVisitor` (or a more specific `TSqlConcreteFragmentVisitor` subclass).
5. XML doc comments on a rule class carry metadata used by the docs generator: `<FriendlyName>`, `<IsIgnorable>`, `<ExampleMd>`. These get parsed into `docs/{Design,Naming,Performance}/SR*.md`.

### Tests

Two test projects:

- `test/SqlServer.Rules.Test` — unit tests for the rules themselves.
  - Each rule has a `SR####Tests.cs` under `Design/`, `Naming/`, or `Performance/`.
  - Tests inherit from `Helpers/TestModel.cs`: add file paths to `TestFiles`, expected hits to `ExpectedProblems`, then `RunTest()`. The helper builds a `TSqlModel` (SQL 2019 / `Sql150`), runs the full analysis service, and uses `CollectionAssert.AreEquivalent` against problems whose `RuleId` starts with `SqlServer.Rules.`.
  - Test SQL inputs live under `sqlprojects/TSQLSmellsTest/` etc. and are referenced via relative paths like `"../../../../../sqlprojects/..."`. **SQL fixture files must be UTF-8 with BOM** or DacFx model loading misbehaves.
  - `SmokeTests/` (TestAw, TestChinook, TestFabric) runs the full ruleset against full sample schemas and asserts the exact problem list — when you add or change a rule, expect these to need updating.
- `test/TSQLAnalyzer.Tests` — covers the analyzer library in `tools/ErikEJ.DacFX.TSQLAnalyzer`.

There is an older `TestCasesBase.cs` / `BaselineSetup.cs` pattern (`GetTestCaseProblems(testCases, ruleId)`) used by a handful of tests; new tests should follow the `TestModel`-based pattern shown in `Design/SRD0039Tests.cs`.

### Tools

- `tools/ErikEJ.DacFX.TSQLAnalyzer` — engine that loads a model from `.sql` / `.dacpac` / `.zip` / live connection, runs DacFx code analysis, applies rule filters (`Rules:-SqlServer.Rules.SRD0004` syntax matching MSBuild.Sdk.SqlProj), and returns `AnalyzerResult`. Used by both the CLI and the VSIX.
- `tools/SqlAnalyzerCli` — `tsqlanalyze` .NET tool (PackageType `McpServer`). Built on Spectre.Console. Doubles as an MCP server when launched with `-mcp` (the `.mcp/server.json` is packed into the nupkg).
- `tools/SqlServer.Rules.DocsGenerator` — **generates `docs/**/*.md` from rule XML doc comments**. Do not hand-edit files in `docs/` — they are regenerated. Includes a hard-coded `MicrosoftRules` table that supplements built-in DacFx `SR####` rules with links to Microsoft Learn.
- `tools/SqlServer.Rules.Generator` — console reporter for rule inventory.
- `tools/SqlServer.Rules.Report` — serialization helpers for analyzer results.
- `tools/SqlAnalyzerVsix` — Visual Studio extension (Windows only).

## Adding a rule

1. Pick the category (Design/Naming/Performance) and the next free `SR{D|N|P}####` id.
2. Add the class in `src/SqlServer.Rules/<Category>/`, inheriting `BaseSqlCodeAnalysisRule`. Use existing files (e.g. `Design/AliasTablesRule.cs`) as the template — they show the `RuleId`/`RuleDisplayName`/`Message` const triple, the `[ExportCodeAnalysisRule]` attribute, and the `<FriendlyName>`/`<IsIgnorable>` XML doc tags the docs generator reads.
3. Constructor passes the relevant element-type set (typically `ProgrammingAndViewSchemas`) to the base. For element rules use `RuleScope = SqlRuleScope.Element`; for whole-model rules use `SqlRuleScope.Model`.
4. Walk the AST with an existing visitor where possible. Return `SqlRuleProblem` instances — the inherited `Problems` list and `MessageFormatter` may help, but most rules build the list locally.
5. Add a matching `SR####Tests.cs` in the parallel folder under `test/SqlServer.Rules.Test/`. Author or reuse a SQL fixture under `sqlprojects/TSQLSmellsTest/` (or another fixture project) and assert the expected `(line, column, ruleId)` triples.
6. If the new rule fires on the smoke-test fixtures (AW / Chinook / Fabric), update the `ExpectedProblems` lists in `test/SqlServer.Rules.Test/SmokeTests/` accordingly.
7. Do **not** hand-edit `docs/` — running `SqlServer.Rules.DocsGenerator` regenerates the markdown.

## Conventions

- The package is signed (`key.snk`). Don't add `InternalsVisibleTo` without matching the public key.
- `Directory.Build.props` enables `EnableNETAnalyzers` with `latest-all`, `GenerateDocumentationFile`, and includes StyleCop. Suppress with attributes/pragmas inline (existing code uses `#pragma warning disable SA####` extensively); don't disable analyzers globally.
- `.editorconfig` at repo root drives formatting — respect it.
- Embedded `src/SqlServer.Rules/EditorConfig.Core/` is a vendored editorconfig parser used by the CLI's `-f` formatter (it's not the build's `.editorconfig`).
