# Static Analysis Rule-sets for SQL Projects

![build status](https://img.shields.io/github/actions/workflow/status/ErikEJ/SqlServer.Rules/pipeline.yml?master) [![latest version](https://img.shields.io/nuget/v/ErikEJ.DacFX.SqlServer.Rules)](https://www.nuget.org/packages/ErikEJ.DacFX.SqlServer.Rules) [![latest version](https://img.shields.io/nuget/v/ErikEJ.DacFX.TSQLSmellSCA)](https://www.nuget.org/packages/ErikEJ.DacFX.TSQLSmellSCA)

## Overview

A library of SQL best practices implement as more than 150 [database code analysis rules](https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/sql-code-analysis?view=sql-server-ver15&pivots=sq1-visual-studio-sdk#sql-project-file-sample-and-syntax) checked at build.

The rules can be added as NuGet packages to SQL projects based on either [MSBuild.Sdk.SqlProj](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj) or [Microsoft.Build.Sql](https://github.com/microsoft/DacFx)

For a complete list of the current rules we have implemented see [here](docs/table_of_contents.md).

> This fork also contains an additional set of rules `TSQL Smells` forked from [TSQL-Smells](https://github.com/davebally/TSQL-Smells)

## Usage

The latest version is available on NuGet

```sh
dotnet add package ErikEJ.DacFX.SqlServer.Rules
```

```sh
dotnet add package ErikEJ.DacFX.TSQLSmellSCA
```

You can read more about using and customizing the rules in the [readme here](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj?tab=readme-ov-file#static-code-analysis)

## Solution Organization

`.github` - GitHub actions

`docs` - markdown files generated from rule inspection with the DocsGenerator unit test

`Solution Items` - files relating to build etc.

`src`

- `SqlServer.Dac` - This hold visitors and other utility code
- `SqlServer.Rules` - This holds the rules derived from `SqlCodeAnalysisRule`
- `TSQLSmellSCA` - an additional set of rules `TSQL Smells` forked from [TSQL-Smells](https://github.com/davebally/TSQL-Smells)

`test`

- `SqlServer.Rules.Generator` - a quick console app to report on all rules in a SQL Project.
- `SqlServer.Rules.Report` - Library for evaluating a rule and serializing the result.
- `SqlServer.Rules.Tests` - a few test to demonstrate unit testing of rules
- `TestDatabase` - a small SQL Database Project with some rule violations
- `TSQLSmellsSSDTTest` - unit tests of some of the `TSQL Smells` rules
- `TSQLSmellsTest` - a SQL Database Project with some rule violations
