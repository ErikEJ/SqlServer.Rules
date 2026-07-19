# T-SQL Analyzer for VS Code

Live T-SQL code analysis for `.sql` files, powered by
[SqlServer.Rules](https://github.com/ErikEJ/SqlServer.Rules) and DacFx. Design,
naming and performance rule violations are surfaced as squiggles as you type.

## How it works

The extension spawns the `ErikEJ.TSQLAnalyzerCli` tool (run via the .NET 10 SDK
`dnx` command) in its hidden long-lived **server mode** (`--server-mode`) and
communicates with it over newline-delimited JSON on stdin/stdout. Document
open/change/save events are debounced and the current (possibly unsaved) buffer
content is sent for analysis; the returned problems are published as
`vscode.Diagnostic` entries.

If the analyzer process crashes it is transparently respawned on the next request,
and the `T-SQL Analyzer: Restart Analysis Server` command can be used to force a
restart.

## Requirements

The [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) must be
installed. The extension runs the analyzer as a NuGet package
(`ErikEJ.DacFX.TSQLAnalyzer.Cli`) via the SDK's `dnx` command, so no separate
install step is required — the package is fetched on first use.

To use a local build instead, point `tsqlAnalyzer.serverPath` at an
`ErikEJ.TSQLAnalyzerCli` executable.

## Settings

| Setting | Description |
| --- | --- |
| `tsqlAnalyzer.enable` | Enable/disable live analysis. |
| `tsqlAnalyzer.serverPath` | Optional path to a build of `ErikEJ.TSQLAnalyzerCli` (empty = run via `dnx`). |
| `tsqlAnalyzer.rules` | Rules expression. All rules are enabled by default; prefix a rule id with `-` to disable it, or with `+!` to promote it to an error, e.g. `-SqlServer.Rules.SRD0004;+!SqlServer.Rules.SRN0005`. Wildcards are supported, e.g. `-SqlServer.Rules.SRN*`. |
| `tsqlAnalyzer.sqlVersion` | Target SQL Server version (e.g. `Sql170`, `SqlAzure`, `SqlDwUnified`). |
| `tsqlAnalyzer.additionalAnalyzers` | Additional analyzer `.dll` paths to load. |
| `tsqlAnalyzer.debounceMs` | Delay after the last edit before analysis runs. |

## Commands

- **T-SQL Analyzer: Analyze Active File**
- **T-SQL Analyzer: Restart Analysis Server**

## Status bar

A T-SQL Analyzer item is shown in the status bar while live analysis is enabled.
It displays a spinning **Analyzing…** indicator while the analyzer is processing
the current buffer, and an idle label otherwise. Clicking it re-analyzes the
active file.

## Building

```bash
npm install
npm run compile
npm run package   # produces tsqlanalyzer.vsix
```
