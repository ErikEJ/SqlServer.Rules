[marketplace]: https://marketplace.visualstudio.com/items?itemName=ErikEJ.TSqlAnalyzer
[ssmsmarketplace]: https://ssmsgallery.azurewebsites.net/extension/TSqlAnalyzerSsms.f1322c34-dfaa-4842-8933-b439626da91d
[vsixgallery]: http://www.vsixgallery.com/extension/SqlAnalyzer.abc6ba2-edd5-4419-8646-a55d0a83f7ff/

# T-SQL Analyzer & SQL Server Rules

[![MCP Toplist](https://mcptoplist.com/badge/glama%2FErikEJ%2FSqlServer.Rules.svg)](https://mcptoplist.com/server/glama%2FErikEJ%2FSqlServer.Rules)

![build status](https://img.shields.io/github/actions/workflow/status/ErikEJ/SqlServer.Rules/vsix.yml?master) [![latest version](https://img.shields.io/nuget/v/ErikEJ.DacFX.SqlServer.Rules)](https://www.nuget.org/packages/ErikEJ.DacFX.SqlServer.Rules) ![GitHub Sponsors](https://img.shields.io/github/sponsors/ErikEJ)

Catch T-SQL design, naming, and performance issues as you type, when you build, or from the command line. T-SQL Analyzer brings over 140 SQL best-practice rules to SQL Server and Azure SQL developers.

**You don't need a SQL Database project to get started** - the live analyzers work with standalone SQL scripts too. All tools use the same core [rule set](docs/readme.md).

## Choose how you work

| Where you work | What you get | Get started |
| --- | --- | --- |
| **Visual Studio** | Live feedback in the Error List while editing SQL scripts | [Install extension][marketplace] · [Setup and options](tools/SqlAnalyzerVsix/readme.md) |
| **SQL Server Management Studio 22** | Live feedback in the Error List while editing SQL scripts | [Install extension][ssmsmarketplace] · [Setup and options](tools/SqlAnalyzerSsms/readme.md) |
| **VS Code** | Live squiggles and diagnostics in `.sql` files | [Install extension](https://marketplace.visualstudio.com/items?itemName=ErikEJ.tsqlanalyzerextension) · [Setup and options](vscode-extension/README.md) |
| **SQL Database projects / CI** | Check rules during builds | [Build-time setup](#check-rules-during-project-builds) |
| **Command line** | Analyze scripts, folders, `.dacpac` files, or live databases | [CLI quick start](#analyze-from-the-command-line) |
| **GitHub Copilot Chat** | Ask Copilot to analyze SQL using the MCP server | [Copilot setup](#use-with-github-copilot) |

## Get live feedback in your editor

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later.
2. Install the T-SQL Analyzer extension for your editor using the links above.
3. Open a SQL script and start editing - analysis runs automatically, with no build step required.

The extensions download the analyzer tooling automatically; no separate CLI or rules package installation is needed. Customize enabled rules and the target SQL Server version in **Tools > Options** (Visual Studio / SSMS) or **Settings** (VS Code). See each extension's setup guide for details.

For Visual Studio, a [CI build][vsixgallery] is also available.

## Check rules during project builds

Add [ErikEJ.DacFX.SqlServer.Rules](https://www.nuget.org/packages/ErikEJ.DacFX.SqlServer.Rules) to your SQL Database project to report issues during local and CI builds.

- **Modern SDK-style projects**: Supported by [MSBuild.Sdk.SqlProj](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj) and [Microsoft.Build.Sql](https://github.com/microsoft/DacFx). Follow the [setup and rule configuration guide](https://rr-wfm.github.io/MSBuild.Sdk.SqlProj/docs/static-code-analysis.html) to add the package and enable analysis.
- **Classic `.sqlproj` projects**: Use the [SSDT setup guide](https://erikej.github.io/dacfx/codeanalysis/sqlserver/2024/04/02/dacfx-codeanalysis.html#addrules) (Visual Studio 2017 or later).

Build-time checks complement live feedback by checking your SQL project as part of your build workflow.

## Analyze from the command line

With the .NET 10 SDK or later installed:

```sh
dotnet tool install --global ErikEJ.DacFX.TSQLAnalyzer.Cli
tsqlanalyze -i C:\scripts\sproc.sql
tsqlanalyze -i C:\scripts\database.dacpac -o results.json
```

Use the CLI for one-off checks or automation, with console, XML, or JSON output. See the [CLI guide](tools/SqlAnalyzerCli/readme.md) for folder and live database analysis, rule filters, and more examples.

## Use with GitHub Copilot

The analyzer also runs as an MCP server, letting GitHub Copilot Chat use the rules to analyze your SQL code in VS Code or Visual Studio. This is separate from automatic live editor feedback.

The VS Code extension includes the MCP server integration - start **T-SQL Analyzer** from the MCP server list to enable it. For standalone MCP installation in either editor, see the [MCP setup guide](tools/SqlAnalyzerCli/readme.md#mcp-server-preview).

## Explore and customize the rules

Browse the [rule catalogue](docs/readme.md) for explanations and examples. You can disable individual rules or groups and promote selected warnings to errors; configuration instructions are linked above for each tool.
