# T-SQL Analyzer CLI

T-SQL Analyzer is a command line tool for identifying, and reporting the presence of anti-patterns in T-SQL scripts. 

It evaluates more than [140 rules](https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/table_of_contents.md) for design, naming and performance issues.

## Getting started

The tool runs on any system with the .NET 8.0 runtime installed. 

### Installing the tool (currently in preview)

```bash
dotnet tool install --global ErikEJ.DacFX.TSQLAnalyzer.Cli --version *-*
```

### Usage

```bash
## Analyze a single file
tsqlanalyze -i C:\scripts\sproc.sql

## Analyze a folder
tsqlanalyze -i "c:\database scripts"

## Analyze a folder with a filter and a full folder path
tsqlanalyze -i c:\database_scripts\sp_*.sql "c:\old scripts"

## Analyze a script with a rule settings filter and for a specific SQL Server version
tsqlanalyze -i C:\scripts\sproc.sql -r Rules:-SqlServer.Rules.SRD0004 -s SqlAzure
```

Rule settings filters are demonstrated [here](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj?tab=readme-ov-file#static-code-analysis)

The SQL Server verison values are documented [here](https://learn.microsoft.com/dotnet/api/microsoft.sqlserver.dac.model.sqlserverversion)

## Sample output

![Sample](https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/cli.png)
