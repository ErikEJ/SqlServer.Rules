# T-SQL Analyzer for VS Code

Catch T-SQL mistakes **as you type**. This extension analyzes your `.sql` files in
real time and highlights design, naming, and performance issues right in the
editor — no build step required.

![diagnostics screenshot](https://raw.githubusercontent.com/ErikEJ/SqlServer.Rules/master/vscode-extension/images/screenshot.png)

## Features

- **Live squiggles** — problems appear while you edit, just like compiler errors
- **100+ built-in rules** — covering design best practices, naming conventions,
  and performance pitfalls
- **Hover for details** — hover over any squiggle to see the rule id, a plain
  English description, and a link to the full documentation
- **Zero configuration** — works out of the box with sensible defaults
- **Customizable** — disable individual rules, promote warnings to errors, or
  target a specific SQL Server version
- **Status bar indicator** — shows when analysis is running

## Getting Started

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
   (or later).
2. Install this extension from the VS Code Marketplace.
3. Open any `.sql` file — analysis starts automatically.

That's it. The analyzer tooling is downloaded automatically on first use; there is
nothing else to install.

## Customizing Rules

All rules are **enabled** by default. You can fine-tune them in
**Settings** using a simple expression:

| What you want | Expression |
| --- | --- |
| Disable a single rule | `-SqlServer.Rules.SRD0004` |
| Disable all naming rules | `-SqlServer.Rules.SRN*` |
| Promote a rule to an error | `+!SqlServer.Rules.SRN0005` |
| Combine several | `-SqlServer.Rules.SRD0004;+!SqlServer.Rules.SRN0005` |

Browse the full rule catalogue at
[github.com/ErikEJ/SqlServer.Rules/docs](https://github.com/ErikEJ/SqlServer.Rules/tree/master/docs).

## Settings

| Setting | Default | Description |
| --- | --- | --- |
| `tsqlAnalyzer.enable` | `true` | Turn live analysis on or off. |
| `tsqlAnalyzer.rules` | *(all enabled)* | Rules expression (see above). |
| `tsqlAnalyzer.sqlVersion` | `Sql170` | Target SQL Server version (`Sql160`, `Sql170`, `SqlAzure`, `SqlDwUnified`, …). |
| `tsqlAnalyzer.debounceMs` | `500` | Milliseconds to wait after the last keystroke before analysing. |
| `tsqlAnalyzer.additionalAnalyzers` | | Extra analyzer `.dll` paths to load (advanced). |
| `tsqlAnalyzer.serverPath` | | Path to a local analyzer build (advanced — leave empty to use the published package). |

## Commands

Open the Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`) and type **T-SQL Analyzer**:

- **T-SQL Analyzer: Analyze Active File** — run analysis on demand.
- **T-SQL Analyzer: Restart Analysis Server** — restart the background analyzer
  process (useful after updating settings).

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later.

## Building from Source

```bash
npm install
npm run compile
npm run package   # produces tsqlanalyzer.vsix
```
