# T-SQL Analyzer CLI

T-SQL Analyzer is a command line tool for identifying, and reporting the presence of anti-patterns in T-SQL scripts. 

It evaluates more than [140 rules](https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/readme.md) for design, naming and performance issues.

## Getting started

The tool runs on any system with the .NET 8.0 runtime installed. 

### Installing the tool

```bash
dotnet tool install --global ErikEJ.DacFX.TSQLAnalyzer.Cli
```

### Usage

```bash
# Analyze all .sql scripts in current folder and sub-folders
tsqlanalyze

## Analyze a single file
tsqlanalyze -i C:\scripts\sproc.sql

## Analyze a folder
tsqlanalyze -i "c:\database scripts"

## Analyze a folder with a filter and a full folder path
tsqlanalyze -i c:\database_scripts\sp_*.sql "c:\old scripts"

## Analyze a script with a rule settings filter and for a specific SQL Server version
tsqlanalyze -i C:\scripts\sproc.sql -r Rules:-SqlServer.Rules.SRD0004 -s SqlAzure

## Analyze a .dacpac
tsqlanalyze -i C:\scripts\Chinook.dacpac

## Analyze a live database
tsqlanalyze -c "Data Source=.\SQLEXPRESS;Initial Catalog=Chinook;Integrated Security=True;Encrypt=false"
```

Rule settings filters are demonstrated [here](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj?tab=readme-ov-file#static-code-analysis)

The SQL Server version values are documented [here](https://learn.microsoft.com/dotnet/api/microsoft.sqlserver.dac.model.sqlserverversion)

## Sample output

The tool will output a summary of the rules that were violated, and the line numbers where the violations occurred.

Table3.sql:

```sql
CREATE TABLE [dbo].[Table3]
(
    [Id] INT NOT NULL, 
    [Wang] NCHAR(500) NOT NULL, 
    [Chung] NCHAR(10) NOT NULL 
)
```

![Sample](https://raw.githubusercontent.com/ErikEJ/SqlServer.Rules/master/docs/cli.png)

## SQL Server Management Studio (SSMS) and Visual Studio integration

You can run the tool against any script in the SQL editor, if configured as an external tool.

From the main menu, select `Tools`, `External Tools` and configure as shown:

![SSMS](https://raw.githubusercontent.com/ErikEJ/SqlServer.Rules/master/docs/ssms.png)

Title: `tsqlanalyze`

Command: `C:\Users\<YourUserName>\.dotnet\tools\tsqlanalyze.exe`

Arguments: `-n -i $(ItemPath)`

Use output window: `✓`

To run the tool, select `Tools`, `tsqlanalyze` and the result will be displayed in the Output window. Double click a warning to navigate to the related script line.

![SSMS](https://raw.githubusercontent.com/ErikEJ/SqlServer.Rules/master/docs/ssmsoutput.png)
