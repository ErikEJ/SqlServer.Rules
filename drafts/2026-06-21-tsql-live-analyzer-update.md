---
layout: post
image: https://raw.githubusercontent.com/ErikEJ/SqlServer.Rules/master/tools/SqlAnalyzerVsix/Images/editor.png
title:  "Updated: T-SQL Analyzer live analysis now in Visual Studio and SSMS"
date:   2026-06-21 18:28:49 +0100
categories: sql dacfx visualstudio ssms
---

The T-SQL Analyzer extensions for Visual Studio and SQL Server Management Studio have been significantly updated. Here's what's new.

### Background

I maintain a collection of over 140 [open source](https://github.com/ErikEJ/SqlServer.Rules) static code analysis rules based on the DacFX API, covering T-SQL best practices for design, naming, and performance.

I previously [blogged about](https://erikej.github.io/sql/dacfx/visualstudio/2025/08/25/dacfx-visx-rules.html) the launch of the T-SQL Analyzer extension for Visual Studio, which provided live analysis of SQL scripts as you author them.

This post covers a significant update to both the Visual Studio and the new SQL Server Management Studio extension.

## What's new

### Now available in SQL Server Management Studio 22

The live analysis experience is no longer limited to Visual Studio — it is now also available in **SQL Server Management Studio 22** (SSMS). You get the same real-time feedback on your SQL scripts as you type, directly in the SSMS query editor.

The SSMS extension is available in the [SSMS VSIX Gallery](https://ssmsgallery.azurewebsites.net/extension/TSqlAnalyzerSsms.f1322c34-dfaa-4842-8933-b439626da91d).

![](https://raw.githubusercontent.com/ErikEJ/SqlServer.Rules/master/tools/SqlAnalyzerSsms/sql-analysis.png)

### Analysis of any SQL script — not just CREATE statements

Previously, live analysis only triggered on SQL scripts that contained `CREATE` statements — the kind typically found inside SQL database projects. The extension has now been revamped to analyze **any SQL script** you have open in the editor, whether it is a stored procedure, a query, a migration script, a one-off data fix, or anything else. If you're writing SQL, you now get feedback.

### Configure via Tools / Options

Both extensions now expose settings under **Tools → Options** in Visual Studio and SSMS, so you can customize behavior without editing project files.

#### Visual Studio

The Visual Studio extension settings are available under **Tools → Options → T-SQL Analyzer**:

| Setting | Description |
|---------|-------------|
| **Run static T-SQL analysis** | Enable or disable live analysis entirely |
| **SQL engine version** | Set the SQL Server dialect used for analysis (SQL Server 2005 through SQL Server 2025, Azure SQL Database, Synapse, Fabric, and more) |
| **Rule exceptions** | Set a rules expression to suppress or enforce specific rules when no SQL project configuration is present (e.g. `+!SqlServer.Rules.SRD0006;-SqlServer.Rules.SRN*`) |

#### SSMS

The SSMS extension settings are available under **Tools → Options → T-SQL Analyzer** (or via the SQL Analyzer category):

| Setting | Description |
|---------|-------------|
| **Disable live code analysis** | Disable live static SQL code analysis |
| **Code Analysis rule settings** | Set the rules expression for live analysis when no SQL project rule configuration is available (e.g. `-SqlServer.Rules.SRD0006;-SqlServer.Rules.SRN*`) |

These options apply globally when a script is **not part of a SQL database project**. When a project is involved, the extension defers to the project configuration (see below).

### Project settings are still respected

If your SQL script is part of a SQL database project (based on [MSBuild.Sdk.SqlProj](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj), [Microsoft.Build.Sql](https://github.com/microsoft/DacFx/tree/main/src/Microsoft.Build.Sql), or a classic `.sqlproj`), the extension continues to respect the project's own configuration:

```xml
<Project Sdk="MSBuild.Sdk.SqlProj/3.2.0">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <SqlServerVersion>Sql170</SqlServerVersion>
    <RunSqlCodeAnalysis>True</RunSqlCodeAnalysis>
    <CodeAnalysisRules>-SqlServer.Rules.SRD0006</CodeAnalysisRules>
    <!-- This property supports wildcard rule filters -->
    <!-- and overrides 'CodeAnalysisRules' above if present -->
    <AnalyzerCodeAnalysisRules>-SqlServer.Rules.SRD0006;-Microsoft.*</AnalyzerCodeAnalysisRules>
  </PropertyGroup>
</Project>
```

This means if analysis is disabled in the project, or if you have suppressed specific rules via `CodeAnalysisRules` or `AnalyzerCodeAnalysisRules`, the live extension honours those settings. The global Tools/Options settings only apply when no project configuration is found.

## Installation

### Visual Studio

Download the extension from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=ErikEJ.TSqlAnalyzer).

- **Visual Studio 2026 and later**: No separate installation required — the extension uses the `dnx` command to run the T-SQL Analyzer CLI as a NuGet package automatically.
- **Visual Studio 2022**: Install the CLI tool first:

```bash
dotnet tool install -g ErikEJ.DacFX.TSQLAnalyzer.CLI
```

### SSMS

Download the extension from the [SSMS VSIX Gallery](https://ssmsgallery.azurewebsites.net/extension/TSqlAnalyzerSsms.f1322c34-dfaa-4842-8933-b439626da91d).

The extension automatically uses the `dnx` command to run the T-SQL Analyzer CLI tool. No separate installation is needed, but the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download) is required (automatically installed with the Database DevOps workload in SSMS).

## Summary

| Feature | Before | Now |
|---------|--------|-----|
| Visual Studio support | ✓ | ✓ |
| SSMS support | ✗ | ✓ |
| Any SQL script analyzed | ✗ (CREATE only) | ✓ |
| Tools/Options settings | ✗ | ✓ |
| Respect SQL project settings | ✓ | ✓ |

## Feedback and contributions

Should you encounter bugs or have feature requests, head over to the [GitHub repo](https://github.com/ErikEJ/SqlServer.Rules) to open an issue if one doesn't already exist.

If you enjoy using the extensions, please give them a ★★★★★ rating on the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=ErikEJ.TSqlAnalyzer).

Another way to help out is to [sponsor me on GitHub](https://github.com/sponsors/ErikEJ).

---

- [GitHub](https://github.com/erikej)
- [X](https://www.x.com/erikej)
