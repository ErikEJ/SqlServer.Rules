# Investigation: Using NuGet Package with Classic .sqlproj

## Issue
Investigate possibility of using the ErikEJ.DacFX.SqlServer.Rules NuGet package with classic .sqlproj files through MSBuild XML configuration.

## Context
- **Related PR**: https://github.com/microsoft/DacFx/pull/479
- **Related Comment**: https://github.com/microsoft/DacFx/pull/479#issuecomment-2349383691
- **Issue**: ErikEJ/SqlServer.Rules#564

## Current State

### Working Scenarios
1. **MSBuild.Sdk.SqlProj**: Modern SDK-style SQL projects work with NuGet package
2. **Microsoft.Build.Sql**: Modern SDK-style SQL projects work with NuGet package

### Manual Installation Required
- Classic .sqlproj files (legacy SSDT format) require manual download and configuration
- Users must manually place DLLs in specific directories
- Configuration is manual and error-prone

## Investigation Goals

1. Understand how classic .sqlproj files are structured
2. Understand how DacFx code analyzers are discovered
3. Determine if MSBuild props/targets can automate analyzer discovery
4. Test if NuGet package can automatically configure classic .sqlproj

## Classic .sqlproj Structure

Classic SQL projects use the old .sqlproj format with:
- `<Import Project="$(MSBuildExtensionsPath)\Microsoft\VisualStudio\v$(VisualStudioVersion)\SSDT\Microsoft.Data.Tools.Schema.SqlTasks.targets" />`
- Properties like `DSP` (Database Schema Provider)
- Properties like `RunSqlCodeAnalysis` (boolean to enable/disable analysis)
- Properties like `CodeAnalysisRules` or `SqlCodeAnalysisRules` (rules configuration)

Example classic .sqlproj structure:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <Name>ClassicSqlProject</Name>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectVersion>4.1</ProjectVersion>
    <ProjectGuid>{...}</ProjectGuid>
    <DSP>Microsoft.Data.Tools.Schema.Sql.Sql160DatabaseSchemaProvider</DSP>
    <OutputType>Database</OutputType>
    <RootPath>
    </RootPath>
    <RootNamespace>ClassicSqlProject</RootNamespace>
    <AssemblyName>ClassicSqlProject</AssemblyName>
    <ModelCollation>1033, CI</ModelCollation>
    <DefaultFileStructure>BySchemaAndSchemaType</DefaultFileStructure>
    <DeployToDatabase>True</DeployToDatabase>
    <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
    <TargetLanguage>CS</TargetLanguage>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <SqlServerVersions>Sql160</SqlServerVersions>
    <DefaultCollation>SQL_Latin1_General_CP1_CI_AS</DefaultCollation>
    <DefaultDatabaseProvider>SqlAzureV12DatabaseSchemaProvider</DefaultDatabaseProvider>
  </PropertyGroup>
  
  <!-- Code Analysis Configuration -->
  <PropertyGroup>
    <RunSqlCodeAnalysis>True</RunSqlCodeAnalysis>
    <SqlCodeAnalysisRules>+!Microsoft.Rules.Data.SR0001</SqlCodeAnalysisRules>
  </PropertyGroup>

  <Import Project="$(MSBuildExtensionsPath)\Microsoft\VisualStudio\v$(VisualStudioVersion)\SSDT\Microsoft.Data.Tools.Schema.SqlTasks.targets" />
</Project>
```

## DacFx Code Analyzer Discovery

DacFx discovers code analyzers through:
1. **Direct Assembly Reference**: Analyzers in specific directories
2. **MEF (Managed Extensibility Framework)**: Scans for `[ExportCodeAnalysisRule]` attributes
3. **Analyzer Paths**: Looks in predefined locations for analyzer assemblies

For NuGet packages, modern SDK-style projects use:
- `analyzers/dotnet/cs/*.dll` path in the NuGet package
- MSBuild automatically adds these to the analysis pipeline

## Proposed Solution

### Option 1: MSBuild Props/Targets Files

Add MSBuild props and targets files to the NuGet package that:
1. Detect if the project is a classic .sqlproj (check for `DSP` property)
2. Add analyzer DLLs to the appropriate property groups
3. Configure code analysis automatically

**Package Structure:**
```
ErikEJ.DacFX.SqlServer.Rules.nupkg
├── analyzers/
│   └── dotnet/
│       └── cs/
│           ├── SqlServer.Rules.dll
│           └── SqlServer.Rules.NetFx.dll
├── build/
│   ├── ErikEJ.DacFX.SqlServer.Rules.props
│   └── ErikEJ.DacFX.SqlServer.Rules.targets
└── buildTransitive/
    ├── ErikEJ.DacFX.SqlServer.Rules.props
    └── ErikEJ.DacFX.SqlServer.Rules.targets
```

**ErikEJ.DacFX.SqlServer.Rules.props:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <!-- Enable code analysis by default if DSP is present (classic .sqlproj indicator) -->
    <RunSqlCodeAnalysis Condition="'$(DSP)' != '' And '$(RunSqlCodeAnalysis)' == ''">True</RunSqlCodeAnalysis>
  </PropertyGroup>
</Project>
```

**ErikEJ.DacFX.SqlServer.Rules.targets:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <!-- Define the analyzer path -->
    <SqlServerRulesPath Condition="'$(SqlServerRulesPath)' == ''">$(MSBuildThisFileDirectory)..\analyzers\dotnet\cs</SqlServerRulesPath>
  </PropertyGroup>

  <!-- Add analyzer assemblies to code analysis for classic .sqlproj -->
  <ItemGroup Condition="'$(DSP)' != ''">
    <SqlCodeAnalysisPath Include="$(SqlServerRulesPath)\SqlServer.Rules.NetFx.dll" Condition="Exists('$(SqlServerRulesPath)\SqlServer.Rules.NetFx.dll')" />
    <SqlCodeAnalysisPath Include="$(SqlServerRulesPath)\SqlServer.Rules.dll" Condition="Exists('$(SqlServerRulesPath)\SqlServer.Rules.dll') And !Exists('$(SqlServerRulesPath)\SqlServer.Rules.NetFx.dll')" />
  </ItemGroup>
</Project>
```

### Option 2: PackageReference Support

Investigate if classic .sqlproj supports `PackageReference` instead of `packages.config`:
- Some versions of SSDT support PackageReference
- Would automatically bring in MSBuild props/targets
- More modern approach but may not be universally supported

### Option 3: NuGet Install Script (Deprecated)

NuGet install.ps1 scripts are deprecated and not recommended for new packages.

## Testing Plan

1. Create a test classic .sqlproj file
2. Add the NuGet package using both:
   - PackageReference (if supported)
   - Manual reference with props/targets
3. Build the project and verify:
   - Code analysis runs
   - Rules are discovered
   - Violations are reported

## Implementation Steps

1. ✅ Document investigation findings
2. ⬜ Create MSBuild props file
3. ⬜ Create MSBuild targets file
4. ⬜ Update SqlServer.Rules.csproj to include props/targets in package
5. ⬜ Create test classic .sqlproj
6. ⬜ Test with classic .sqlproj
7. ⬜ Update documentation

## Research Notes

### DacFx PR #479 Insights
The PR demonstrates packaging code analyzers in the `analyzers/dotnet/cs` path, which is the standard location for Roslyn/MSBuild analyzers.

### Classic .sqlproj Code Analysis Properties
- `RunSqlCodeAnalysis`: Boolean to enable/disable analysis
- `CodeAnalysisRules` or `SqlCodeAnalysisRules`: Semicolon-separated list of rules
- `SqlCodeAnalysisPath`: Additional paths to search for analyzer assemblies

### Key Challenge
Classic .sqlproj uses `$(MSBuildExtensionsPath)\Microsoft\VisualStudio\v$(VisualStudioVersion)\SSDT\Microsoft.Data.Tools.Schema.SqlTasks.targets` which may not automatically recognize NuGet-provided analyzers like modern SDK-style projects do.

The solution needs to explicitly add analyzer paths through MSBuild properties/items.

## References
- [DacFx PR #479](https://github.com/microsoft/DacFx/pull/479)
- [MSBuild.Sdk.SqlProj Static Code Analysis](https://github.com/rr-wfm/MSBuild.Sdk.SqlProj?tab=readme-ov-file#static-code-analysis)
- [SQL Database Projects Overview](https://learn.microsoft.com/sql/tools/sql-database-projects/get-started)
