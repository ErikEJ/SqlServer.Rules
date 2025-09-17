# SqlServer.Rules - SQL Code Analysis Library

SqlServer.Rules is a .NET library and command line tool that provides static code analysis for SQL Server projects, implementing more than 140 rules for design, naming, and performance analysis. The solution includes NuGet packages, a CLI tool, and a Visual Studio extension for analyzing SQL code against best practices.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap and Build Process
- Prerequisites: .NET 8.0 or later SDK
- Clone repository and build:
  - `dotnet restore` -- NEVER CANCEL. Takes approximately 25 seconds. Set timeout to 60+ seconds.
  - `dotnet build --no-restore --configuration Release` -- NEVER CANCEL. Takes approximately 26 seconds. Set timeout to 90+ seconds.

### Testing
- Run unit tests:
  - `dotnet test --no-build --configuration Release --verbosity normal` -- NEVER CANCEL. Takes approximately 27 seconds. Set timeout to 90+ seconds.
- Tests run across multiple projects: SqlServer.Rules.Tests, TSQLSmellsSSDTTest, TSQLAnalyzer.Tests

### CLI Tool Development and Testing
- Build CLI tool: Build is included in main solution build
- Test CLI tool functionality:
  - Direct execution: `./tools/SqlAnalyzerCli/bin/Release/net8.0/ErikEJ.TSQLAnalyzerCli -i [file]`
  - Package CLI: `dotnet pack tools/SqlAnalyzerCli/SqlAnalyzerCli.csproj --configuration Release`
  - Install globally: `dotnet tool install --global ErikEJ.DacFX.TSQLAnalyzer.Cli`
  - Test samples: Use `tools/SqlAnalyzerCli/testfiles/simple.sql` or `tools/SqlAnalyzerCli/testfiles/Chinook.dacpac`

### Visual Studio Extension (VSIX)
- CANNOT BE BUILT ON LINUX: Requires Windows Desktop frameworks (WPF)
- Build on Windows only with: `msbuild tools/SqlAnalyzerVsix/SqlAnalyzerVsix.csproj /property:Configuration=Release`
- Do not attempt VSIX builds in Linux environments

## Validation

### Manual Validation Requirements
- ALWAYS test CLI functionality with actual SQL files after making changes
- Test CLI with both .sql files and .dacpac files
- Verify rule analysis produces expected warnings/errors
- Example validation commands:
  - `tsqlanalyze -i tools/SqlAnalyzerCli/testfiles/simple.sql` (should produce 7 warnings)
  - `tsqlanalyze -i tools/SqlAnalyzerCli/testfiles/Chinook.dacpac` (should produce 51+ warnings)

### CI Validation
- GitHub Actions pipeline builds and tests on ubuntu-latest
- VSIX builds separately on windows-latest
- All tests must pass before merging changes

## Project Structure

### Core Libraries (`src/`)
- `SqlServer.Rules` - Main rule implementations using DacFx SqlCodeAnalysisRule
- `SqlServer.TSQLSmells` - Additional TSQL Smells rules (forked from davebally/TSQL-Smells)

### Tools (`tools/`)
- `SqlAnalyzerCli` - Command line interface for rule analysis (.NET tool)
- `SqlAnalyzerVsix` - Visual Studio extension (Windows only)
- `ErikEJ.DacFX.TSQLAnalyzer` - Core analyzer library used by CLI
- `SqlServer.Rules.Generator` - Utility for generating rule documentation
- `SqlServer.Rules.Report` - Library for result serialization

### Test Projects (`test/`)
- `SqlServer.Rules.Test` - Unit tests for core rules
- `TSQLSmellsSSDTTest` - Tests for TSQL Smells rules  
- `TSQLAnalyzer.Tests` - Tests for analyzer library
- `TestHelpers` - Shared test infrastructure

### Sample SQL Projects (`sqlprojects/`)
- `AW` - AdventureWorks schema for validation
- `Chinook` - Chinook database schema for testing
- `TestDatabase` - Small test database with rule violations
- `TSQLSmellsTest` - Sample project with TSQL smell violations

### Documentation (`docs/`)
- `Design/` - Documentation for design rules (SRD* series)
- `Performance/` - Documentation for performance rules (SRP* series)  
- `Naming/` - Documentation for naming rules (SRN* series)
- `CodeSmells/` - Documentation for code smell rules (SML* series)

## Common Tasks

### Creating New Analysis Rules
- Inherit from `SqlCodeAnalysisRule` (see existing examples in `src/SqlServer.Rules/`)
- Add unit tests in appropriate category under `test/SqlServer.Rules.Test/`
- Use `TestModel` helper class for test setup
- Generate documentation with `SqlServer.Rules.Generator`

### CLI Tool Development
- CLI built on Spectre.Console for rich terminal output
- Supports multiple input formats: .sql files, .dacpac files, live databases, .zip archives
- Key classes: `Program.cs`, `AnalyzerFactory`, `DisplayService`
- Test changes with sample files in `tools/SqlAnalyzerCli/testfiles/`

### Rule Testing Pattern
```csharp
[TestMethod]
public void TestRuleName()
{
    var problems = GetTestCaseProblems(nameof(RuleClass), RuleClass.RuleId);
    Assert.AreEqual(expectedCount, problems.Count);
    // Verify specific problem details
}
```

### Working with SQL Projects
- SQL projects use MSBuild.Sdk.SqlProj or Microsoft.Build.Sql
- DACPAC files can be analyzed directly with CLI tool
- Sample databases in `sqlprojects/` for testing rule scenarios

## Troubleshooting

### Build Issues
- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` before building
- VSIX builds require Windows environment

### Test Failures
- Check test SQL files are UTF-8 encoded with BOM
- Verify test baseline files match expected output
- Run individual test categories: `dotnet test --filter TestCategory=Performance`

### CLI Tool Issues
- CLI may show null reference on `--help` (known issue)
- Test with actual files: `tsqlanalyze -i [filepath]`
- Check tool version: Latest published version may differ from local build

Remember: This codebase analyzes SQL code quality using Microsoft DacFx. Always test rule changes against real SQL scenarios and ensure new rules follow existing patterns for consistency.