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

### For Classic .sqlproj Projects (manual, no NuGet reference)

If your classic SSDT project does not consume the package directly, you can manually extract and use the analyzer DLL:

1. Download the `ErikEJ.DacFX.SqlServer.Rules` NuGet package (`.nupkg`) from nuget.org.
2. Extract `analyzers/dotnet/cs/SqlServer.Rules.NetFx.dll`.
3. Copy `SqlServer.Rules.NetFx.dll` to the same folder as your `.sqlproj` file.
4. Add the following to your `.sqlproj` file:

```xml
<PropertyGroup>
  <RunSqlCodeAnalysis>True</RunSqlCodeAnalysis>
</PropertyGroup>

<ItemGroup>
  <!-- Keep the analyzer DLL as content in the project folder -->
  <Content Include="SqlServer.Rules.NetFx.dll">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>

<Target Name="AddSqlServerRulesAnalyzerPath" BeforeTargets="Build"
        Condition="Exists('$(MSBuildProjectDirectory)\SqlServer.Rules.NetFx.dll')">
  <ItemGroup>
    <SqlCodeAnalysisPath Include="$(MSBuildProjectDirectory)\SqlServer.Rules.NetFx.dll" />
  </ItemGroup>
</Target>
```

### Configuration

Configure code analysis in your project file:

```xml
<PropertyGroup>
  <!-- Enable/disable analysis -->
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
