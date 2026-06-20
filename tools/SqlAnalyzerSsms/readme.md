[ssmsmarketplace]: https://ssmsgallery.azurewebsites.net/extension/TSqlAnalyzerSsms.f1322c34-dfaa-4842-8933-b439626da91d
[repo]:https://github.com/ErikEJ/SqlServer.Rules

# T-SQL Analyzer

### Live Code Analysis for SQL scripts in SQL Server Management Studio 22

[![Build](https://github.com/ErikEJ/SqlServer.Rules/actions/workflows/vsix.yml/badge.svg)](https://github.com/ErikEJ/SqlServer.Rules/actions/workflows/vsix.yml)
![GitHub Sponsors](https://img.shields.io/github/sponsors/ErikEJ)

Download the latest build of this extension from the [SSMS VSIX Gallery][ssmsmarketplace]

----------------------------------------

Analyze your SQL scripts as you type, and get suggestions for improvements based on best practices. The analyzer has over [140 rules](https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/readme.md) covering performance, security, maintainability, and more.

![editor](Images/editor.png)

The extension works with both SQL Server and Azure SQL Database projects based on [MSBuild.Sdk.SqlProj](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj) or [Microsoft.Build.Sql](https://github.com/microsoft/DacFx/tree/main/src/Microsoft.Build.Sql) as well as classic [SQL database projects](https://learn.microsoft.com/sql/tools/sql-database-projects/get-started?view=sql-server-ver17&pivots=sq1-visual-studio).

The extension will respect any rule configuration you have in your SQL project, including whether analysis is enabled, SQL version and rule suppression.

```xml
<Project Sdk="MSBuild.Sdk.SqlProj/3.2.0">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <SqlServerVersion>Sql170</SqlServerVersion>
    <RunSqlCodeAnalysis>True</RunSqlCodeAnalysis>
    <CodeAnalysisRules>-SqlServer.Rules.SRD0006</CodeAnalysisRules>
    <!-- This property can be added to your .sqlproj file to support wildcard rule filters, -->
    <!-- will override 'CodeAnalysisRules' above if present -->
    <AnalyzerCodeAnalysisRules>-SqlServer.Rules.SRD0006;-Microsoft.*</AnalyzerCodeAnalysisRules>
  </PropertyGroup>
</Project>
```

In addition, the extension supports analysis of any SQL script in your editor, whether it is part of a project or not.

### Installation Requirements

- The [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download) is required to run the T-SQL Analyzer CLI tool, which is used by the extension to perform analysis. The SDK is automatically installed with the Database DevOps workload in SSMS.

## How can I help?

Should you encounter bugs or have feature requests, head over to the [GitHub repo][repo] to open an issue if one doesn't already exist.

Another way to help out is to [sponsor me on GitHub](https://github.com/sponsors/ErikEJ).

If you would like to contribute code, please fork the [GitHub repo][repo] and submit a pull request.
