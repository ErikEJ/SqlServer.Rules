# T-SQL Analyzer CLI

T-SQL Analyzer is a command line tool for identifying, and reporting the presence of anti-patterns in T-SQL scripts.

It evaluates more than [140 rules](https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/readme.md) for design, naming and performance issues.

## Getting started

The tool runs on any system with the .NET 10.0 SDK (or later) installed.

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

## Analyze a .dacpac and save results to an xml file
tsqlanalyze -i C:\scripts\Chinook.dacpac -o result.xml

## Analyze a .dacpac and save results to a JSON file
tsqlanalyze -i C:\scripts\Chinook.dacpac -o result.json

## Analyze a .zip file with .sql files
tsqlanalyze -i C:\scripts\Fabric.zip

## Analyze a live database
tsqlanalyze -c "Data Source=.\SQLEXPRESS;Initial Catalog=Chinook;Integrated Security=True;Encrypt=false"

## Analyze a single file, and include path(s) to your own rules .dll file(s)
tsqlanalyze -i C:\scripts\sproc.sql -a C:\code\analyzers

## Analyze a single file, and reformat (PREVIEW)
tsqlanalyze -i C:\scripts\sproc.sql -f 
```

## SRD0097 sensitivity policy evaluation

SRD0097 continues to check classified columns as before. To additionally identify potentially sensitive columns by name, explicitly select an SSMS Information Protection policy (version 2.0):

```bash
tsqlanalyze -i C:\scripts\database.sql --sensitivity-policy C:\config\sensitivity-policy.json
```

An optional exact-column exceptions file suppresses only policy-derived findings:

```bash
tsqlanalyze -i C:\scripts\database.dacpac \
  --sensitivity-policy C:\config\sensitivity-policy.json \
  --sensitivity-exceptions C:\config\sensitivity-exceptions.json
```

Source SQL files and editor buffers can opt in through `.editorconfig`; relative paths are resolved from the `.editorconfig` file that defines each setting:

```ini
[*.sql]
sqlserver_rules.srd0097.policy_file = config/sensitivity-policy.json
sqlserver_rules.srd0097.exceptions_file = config/sensitivity-exceptions.json
```

The policy is never selected or downloaded automatically. Policy matches use column names and SQL `LIKE`-style `%`, `_`, and bracket patterns. Numeric columns are ignored for keywords with `CanBeNumeric: false`. A keyword with `Excluded: true` is ignored; it does not cancel matches from other keywords. Matching does not add classification metadata to the model. Exceptions use the exact `[schema].[table].[column]` form and require a non-empty `reason`; optional `expires` dates use `yyyy-MM-dd` and remain valid through that UTC date. An exceptions file requires a selected policy. Invalid selected files are reported as analysis errors rather than silently disabling policy evaluation.

Rule settings filters are demonstrated [here](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj?tab=readme-ov-file#static-code-analysis)

The SQL Server version values are documented [here](https://learn.microsoft.com/dotnet/api/microsoft.sqlserver.dac.model.sqlserverversion)

> Note: UTF-8 with BOM encoding is required for the input files.

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

## MCP Server

You can use the tool to ask GitHub Copilot to analyze your SQL Server T-SQL scripts in Visual Studio, SSMS or VS Code.

The TSQL Analyzer MCP Server supports quick installation across multiple development environments. Choose your preferred client below for streamlined setup:

| Client | One-click Installation | MCP Guide |
|--------|----------------------|-------------------|
| **VS Code** | [![Install in VS Code](https://img.shields.io/badge/VS_Code-Install_TSQLAnalyzer-0098FF?style=flat-square&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=tsqlanalyzer&config=%7B%22name%22%3A%22tsqlanalyzer%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22ErikEJ.DacFX.TSQLAnalyzer.Cli%22%2C%22--yes%22%2C%22--%22%2C%22-mcp%22%5D%7D) | [VS Code MCP Official Guide](https://code.visualstudio.com/docs/copilot/chat/mcp-servers) |
| **Visual Studio** | [![Install in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_TSQLAnalyzer-C16FDE?logo=visualstudio&logoColor=white)](https://vs-open.link/mcp-install?%7B%22name%22%3A%22tsqlanalyzer%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22ErikEJ.DacFX.TSQLAnalyzer.Cli%22%2C%22--yes%22%2C%22--%22%2C%22-mcp%22%5D%7D) | [Visual Studio MCP Official Guide](https://learn.microsoft.com/visualstudio/ide/mcp-servers) |

**Example JSON configuration:**

```json
{
    "servers": {
        "tsqlanalyzer": {
            "type": "stdio",
            "command": "dnx",
            "args": [
                "ErikEJ.DacFX.TSQLAnalyzer.Cli",
                "--yes",
                "--",
                "-mcp"
            ]
        }
    }
}
```

## SQL Formatting (preview)

Inspired by the [SQL Formatter](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.SqlFormatter) Visual Studio extension from [Mads Kristensen](https://github.com/madskristensen), the `-f` switch will enable formatting of the analyzed scripts using the [ScriptDom library](https://www.nuget.org/packages/Microsoft.SqlServer.TransactSql.ScriptDom).

- Formats T-SQL code to a consistent and readable layout
- Customizable formatting rules
- `.editorconfig` support

See [this](https://github.com/madskristensen/SqlFormatter?tab=readme-ov-file#editorconfig-support) for information about the `.editorconfig` format.

See [this](https://learn.microsoft.com/dotnet/api/microsoft.sqlserver.transactsql.scriptdom.sqlscriptgeneratoroptions?view=sql-transactsql-161#properties) for information about the available options.

> Enabling this will cause your script files to be updated!

mcp-name: io.github.ErikEJ/SqlServer.Rules
