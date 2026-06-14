# SqlServer.Rules

This project implements a number of SQL Server static code analysis rules, and publishes these as a NuGet package.

## Supported Project Types

The NuGet package works with:
- **Modern SDK-style projects**: [Microsoft.Build.Sql](https://github.com/microsoft/DacFx/tree/main/src/Microsoft.Build.Sql) and [MSBuild.Sdk.SqlProj](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj)
- **Classic .sqlproj**: Legacy SSDT projects (requires Visual Studio 2017+ with SSDT)

## Getting Started

### For Modern SDK-style Projects

Add the NuGet package to your project:

```sh
dotnet add package ErikEJ.DacFX.SqlServer.Rules
```

The rules will automatically run during build. See [MSBuild.Sdk.SqlProj documentation](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj?tab=readme-ov-file#static-code-analysis) for advanced configuration.

### For Classic .sqlproj Projects

#### Using PackageReference (Visual Studio 2017+)

Add to your .sqlproj file:

```xml
<ItemGroup>
  <PackageReference Include="ErikEJ.DacFX.SqlServer.Rules" Version="5.0.0" />
</ItemGroup>
```

Code analysis will run automatically during build.

#### Using packages.config

Install via NuGet Package Manager or Package Manager Console:

```powershell
Install-Package ErikEJ.DacFX.SqlServer.Rules
```

The package will automatically configure itself for classic .sqlproj projects.

### Configuration

Configure code analysis in your project file:

```xml
<PropertyGroup>
  <!-- Enable/disable analysis (enabled by default for classic .sqlproj) -->
  <RunSqlCodeAnalysis>True</RunSqlCodeAnalysis>
  
  <!-- Configure which rules to run -->
  <SqlCodeAnalysisRules>+!SqlServer.Rules.SRD0006</SqlCodeAnalysisRules>
</PropertyGroup>
```

### Troubleshooting

Enable verbose output to verify analyzers are loaded:

```xml
<PropertyGroup>
  <SqlServerRulesVerbose>True</SqlServerRulesVerbose>
</PropertyGroup>
```

## More Information

See my blog post [here](https://erikej.github.io/dacfx/codeanalysis/sqlserver/2024/04/02/dacfx-codeanalysis.html) for a detailed guide on using these rules in your SQL projects.
