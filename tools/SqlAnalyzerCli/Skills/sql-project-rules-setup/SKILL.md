---
name: sql-project-rules-setup
description: Configure SqlServer.Rules code analysis for an MSBuild.Sdk.SqlProj SQL project in Visual Studio.
---

# Configure SqlServer.Rules in an MSBuild.Sdk.SqlProj project

Use this workflow when asked to enable or configure SQL code analysis in an existing
MSBuild.Sdk.SqlProj project. Inspect the project first and preserve its SDK version,
target framework, SQL Server version, references, and existing rule settings.
Do not apply these instructions to a classic SSDT or Microsoft.Build.Sql project
without checking its corresponding documentation.

1. In Visual Studio, open the solution and edit the SDK-style project file (`.csproj`
   or `.sqlproj`). Confirm its `Project` element uses `MSBuild.Sdk.SqlProj`.
2. Add the `ErikEJ.DacFX.SqlServer.Rules` NuGet package to that project using
   Visual Studio's NuGet Package Manager, or add a `PackageReference` with a
   compatible published version. If it is already referenced, do not add a duplicate.
   Respect central package version management if the solution uses it.
3. Set `RunSqlCodeAnalysis` to `True` in the project properties, updating an existing
   value instead of adding a conflicting one:

   ```xml
   <PropertyGroup>
     <RunSqlCodeAnalysis>True</RunSqlCodeAnalysis>
   </PropertyGroup>
   ```

4. Restore and build the project in Visual Studio, and inspect the Error List or
   build output for `SqlServer.Rules` diagnostics. Fix a reported issue and rebuild
   to verify analysis is running; a clean build alone does not prove that the rules ran.
5. If customization is requested, preserve existing `CodeAnalysisRules` entries.
   For example, `-SqlServer.Rules.SRD0004` disables one rule and
   `+!SqlServer.Rules.SRN0005` promotes a rule to an error. Separate entries with
   semicolons. Do not disable rules merely to make the build pass.

For SDK-specific setup and configuration, see
https://rr-wfm.github.io/MSBuild.Sdk.SqlProj/docs/static-code-analysis.html.
For rule descriptions, see https://github.com/ErikEJ/SqlServer.Rules/tree/master/docs.
