# SqlServer.Rules - SQL Code Analysis Library

SqlServer.Rules is a .NET library and command line tool that provides static code analysis for SQL Server projects, implementing design, naming, and performance analysis rules. The solution includes NuGet packages, a CLI tool, and a Visual Studio extension for analyzing SQL code against best practices.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap and Build Process
- Prerequisites: .NET 10.0 SDK or later
- The solution file is `SqlServer.Rules.slnx`
- On Windows, restore and build the full solution:
  - `dotnet restore SqlServer.Rules.slnx` -- NEVER CANCEL. Takes approximately 25 seconds. Set timeout to 60+ seconds.
  - `dotnet build SqlServer.Rules.slnx --no-restore --configuration Release` -- NEVER CANCEL. Set timeout to 90+ seconds.
- On Linux/macOS, do not run `dotnet build` at the repository root. The solution includes Windows-only extension projects (`tools/SqlAnalyzerVsix`, `tools/SqlAnalyzerSsms`) that fail to build outside Windows. Use project-specific build/test commands instead.

### Testing
- On Windows, run the full test command used by CI:
  - `dotnet test --no-build --configuration Release --verbosity normal` -- NEVER CANCEL. Takes approximately 30 seconds. Set timeout to 90+ seconds.
- Always run `SqlServer.Rules.Tests` for pull requests:
  - `dotnet test test/SqlServer.Rules.Test/SqlServer.Rules.Tests.csproj --configuration Release --verbosity normal`
- `TSQLAnalyzer.Tests` covers the analyzer library:
  - `dotnet test test/TSQLAnalyzer.Tests/TSQLAnalyzer.Tests.csproj --configuration Release --verbosity normal`

### CLI Tool Development and Testing
- Build CLI tool: Included in the Windows solution build; on Linux/macOS build `tools/SqlAnalyzerCli/SqlAnalyzerCli.csproj` directly if needed
- Test CLI tool functionality:
  - Direct execution: `./tools/SqlAnalyzerCli/bin/Release/net10.0/ErikEJ.TSQLAnalyzerCli -i [file]`
  - Package CLI: `dotnet pack tools/SqlAnalyzerCli/SqlAnalyzerCli.csproj --configuration Release`
  - Install globally: `dotnet tool install --global ErikEJ.DacFX.TSQLAnalyzer.Cli`
  - Test samples: Use `tools/SqlAnalyzerCli/testfiles/simple.sql` or `tools/SqlAnalyzerCli/testfiles/Chinook.dacpac`

### Visual Studio Extension (VSIX)
- `tools/SqlAnalyzerVsix` and `tools/SqlAnalyzerSsms` are Windows-only extension projects
- Do not attempt extension builds in Linux environments
- Build the solution extensions on Windows with:
  - `msbuild SqlServer.Rules.slnx /property:Configuration=Release /p:DeployExtension=false`

## Validation

### Manual Validation Requirements
- ALWAYS test CLI functionality with actual SQL files after making changes
- Test CLI with both .sql files and .dacpac files
- Verify rule analysis produces expected warnings/errors
- Treat the sample problem counts below as the current verified baseline; update them if rule changes intentionally alter the results
- Example validation commands:
  - `./tools/SqlAnalyzerCli/bin/Release/net10.0/ErikEJ.TSQLAnalyzerCli -i tools/SqlAnalyzerCli/testfiles/simple.sql || true` (currently reports 5 problems and returns exit code 1 because findings were reported)
  - `./tools/SqlAnalyzerCli/bin/Release/net10.0/ErikEJ.TSQLAnalyzerCli -i tools/SqlAnalyzerCli/testfiles/Chinook.dacpac || true` (currently reports 45 problems)

### CI Validation
- GitHub Actions library/CLI workflows run on windows-latest
- VSIX and SSMS extension builds run separately on windows-latest
- New pull requests must run `SqlServer.Rules.Tests`
- Run `TSQLAnalyzer.Tests` too when changing the analyzer library or CLI

## Project Structure

### Core Libraries (`src/`)
- `SqlServer.Rules` - Main rule implementations using DacFx SqlCodeAnalysisRule

### Tools (`tools/`)
- `SqlAnalyzerCli` - Command line interface for rule analysis (.NET tool)
- `SqlAnalyzerSsms` - SSMS extension (Windows only)
- `SqlAnalyzerVsix` - Visual Studio extension (Windows only)
- `ErikEJ.DacFX.TSQLAnalyzer` - Core analyzer library used by CLI
- `ErikEJ.DacFX.TSQLAnalyzer.Protocol` - Shared protocol models used by analyzer tooling
- `SqlServer.Rules.DocsGenerator` - Generates `docs/**/*.md` from rule metadata
- `SqlServer.Rules.Generator` - Utility for reporting available rules
- `SqlServer.Rules.Report` - Library for result serialization

### VS Code Extension (`vscode-extension/`)
- TypeScript extension providing live T-SQL analysis in VS Code
- Communicates with the CLI tool in server mode over stdin/stdout JSON protocol
- Key files: `src/extension.ts` (activation & lifecycle), `src/client.ts` (server-mode client)
- Build with `npm install && npm run compile` from the `vscode-extension/` directory
- Package with `npm run package` (produces `tsqlanalyzer.vsix`)
- Not part of the .NET solution — built and packaged separately

### Test Projects (`test/`)
- `SqlServer.Rules.Test` - Unit tests for core rules
- `TSQLAnalyzer.Tests` - Tests for analyzer library

### Sample SQL Projects (`sqlprojects/`)
- `AW` - AdventureWorks schema for validation
- `Chinook` - Chinook database schema for testing
- `TestDatabase` - Small test database with rule violations
- `TSQLSmellsTest` - Sample project with TSQL smell violations 

### Documentation (`docs/`)
- `Design/` - Documentation for design rules (SRD* series)
- `Performance/` - Documentation for performance rules (SRP* series)  
- `Naming/` - Documentation for naming rules (SRN* series)

**IMPORTANT**: Never make changes to any files in the `docs/` folder. This documentation is automatically generated by `tools/SqlServer.Rules.DocsGenerator` and should not be manually modified.

## Common Tasks

### Creating New Analysis Rules
- Inherit from `BaseSqlCodeAnalysisRule` (see existing examples in `src/SqlServer.Rules/`)
- Add unit tests in appropriate category under `test/SqlServer.Rules.Test/`
- Use `TestModel` helper class for test setup
- Generate documentation with `SqlServer.Rules.DocsGenerator`

### CLI Tool Development
- CLI built on Spectre.Console for rich terminal output
- Supports multiple input formats: .sql files, .dacpac files, live databases, .zip archives
- Key classes: `Program.cs`, `AnalyzerFactory`, `DisplayService`
- Test changes with sample files in `tools/SqlAnalyzerCli/testfiles/`

### Rule Testing Pattern
Test fixture paths are typically referenced relative to `test/SqlServer.Rules.Test/`, which is why examples use deep `../../../../../` paths into `sqlprojects/`.

```csharp
[TestMethod]
public void TestRuleName()
{
    TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/Example.sql");

    ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0000"));

    RunTest();
}
```

### Working with SQL Projects
- SQL projects use MSBuild.Sdk.SqlProj or Microsoft.Build.Sql
- DACPAC files can be analyzed directly with CLI tool
- Sample databases in `sqlprojects/` for testing rule scenarios

## Troubleshooting

### Build Issues
- Ensure .NET 10.0 SDK is installed
- Run `dotnet restore` before building
- Full solution builds require Windows because the solution includes `SqlAnalyzerVsix` and `SqlAnalyzerSsms`

### Test Failures
- Check test SQL files are UTF-8 encoded with BOM
- Verify test baseline files match expected output
- Run individual test categories: `dotnet test --filter TestCategory=Performance`

### CLI Tool Issues
- Test with actual files: `tsqlanalyze -i [filepath]`
- Check tool version: Latest published version may differ from local build

Remember: This codebase analyzes SQL code quality using Microsoft DacFx. Always test rule changes against real SQL scenarios and ensure new rules follow existing patterns for consistency.
