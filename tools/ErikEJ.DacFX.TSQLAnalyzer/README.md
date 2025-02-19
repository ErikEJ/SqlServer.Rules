[![NuGet](https://img.shields.io/nuget/v/ErikEJ.DacFX.TSQLAnalyzer)](https://www.nuget.org/packages/ErikEJ.DacFX.TSQLAnalyzer)

# ErikEJ.DacFX.TSQLAnalyzer

This .NET 8 library allows you to run 140+ static T-SQL code analysis rules against .sql files, and report any rule violations.

## Installation

Install the latest package from [NuGet](https://www.nuget.org/packages/ErikEJ.SqlClient.Extensions).

## Getting started

Once installed, you can use the library to analyze T-SQL files. The library comes with a couple of useful extension methods to help you format the output of the analysis. 

```csharp
using ErikEJ.DacFX.TSQLAnalyzer;
using ErikEJ.DacFX.TSQLAnalyzer.Extensions;

var files = new List<string> { "C:\\scripts\\sproc.sql" };

var analyzerOptions = new AnalyzerOptions();

analyzerOptions.Scripts.AddRange(files);

var analyzerFactory = new AnalyzerFactory(analyzerOptions);

AnalyzerResult result;

try
{
    result = analyzerFactory.Analyze();
}
catch (ArgumentException aex)
{
    Console.WriteLine(aex.Message);
    return 1;
}

if (result?.Result == null)
{
    Console.WriteLine("No result from analysis");
    return 1;
}

foreach (var err in result.Result.InitializationErrors)
{
    Console.WriteLine(err.Message);
}

foreach (var err in result.Result.SuppressionErrors)
{
    Console.WriteLine(err.Message);
}

foreach (var err in result.Result.AnalysisErrors)
{
    Console.WriteLine(err.Message);
}

if (result.ModelErrors.Count > 0)
{
    foreach (var dex in result.ModelErrors)
    {
        Console.WriteLine(dex.Value.Format(dex.Key));
    }
}

if (result.Result.AnalysisSucceeded)
{
    foreach (var err in result.Result.Problems)
    {
        var warning = err.GetOutputMessage(analyzerOptions.Rules);

        Console.WriteLine(warning);
    }
}
```
