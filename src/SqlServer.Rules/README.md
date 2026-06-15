# SqlServer.Rules

This project implements a number of SQL Server static code analysis rules, and publishes these as a NuGet package.

## Supported Project Types

The NuGet package works with:
- **Modern SDK-style projects**: [Microsoft.Build.Sql](https://github.com/microsoft/DacFx/tree/main/src/Microsoft.Build.Sql) and [MSBuild.Sdk.SqlProj](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj)
- **Classic .sqlproj**: Legacy SSDT projects - requires Visual Studio with SQL Server Data Tools (SSDT) and manual setup to use the analyzer DLL.

## Getting Started

### For Modern SDK-style Projects

Add the NuGet package to your project:

```sh
dotnet add package ErikEJ.DacFX.SqlServer.Rules
```

 Add the following to your `.csproj` or `.sqlproj` file:

```xml
<PropertyGroup>
  <RunSqlCodeAnalysis>True</RunSqlCodeAnalysis>
</PropertyGroup>
```

The rules will automatically run during build. See our [MSBuild.Sdk.SqlProj documentation](https://rr-wfm.github.io/MSBuild.Sdk.SqlProj/docs/static-code-analysis.html) for advanced configuration.

## More Information

See my blog post [here](https://erikej.github.io/dacfx/codeanalysis/sqlserver/2024/04/02/dacfx-codeanalysis.html) for a detailed guide on using these rules in your SQL projects.
