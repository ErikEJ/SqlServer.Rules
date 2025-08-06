using System.Diagnostics;
using CommandLine;
using CommandLine.Text;
using ErikEJ.DacFX.TSQLAnalyzer;
using ErikEJ.DacFX.TSQLAnalyzer.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using SqlAnalyzerCli.Services;

namespace ErikEJ.SqlAnalyzer;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 1 && args[0] == "-mcp")
            {
                var builder = Host.CreateApplicationBuilder(args);

                builder.Services.AddMcpServer()
                .WithStdioServerTransport()
                .WithTools<AnalyzerTools>();

                builder.Logging.AddConsole(options =>
                {
                    options.LogToStandardErrorThreshold = LogLevel.Trace;
                });

                await builder.Build().RunAsync();

                return 0;
            }
            else if (args.Length == 1 && args[0] == "-mcp-test")
            {
                await AnalyzerTools.FindSqlScriptProblems("CREATE PROCEDURE [dbo].[TestProc] AS SELECT * FROM dbo.TestTable;");
                return 1;
            }
            else
            {
                return await MainAsync(args).ConfigureAwait(false);
            }
        }
#pragma warning disable CA1031
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex.Demystify(), ExceptionFormats.ShortenPaths);
            return 1;
        }
#pragma warning restore CA1031
    }

    public static async Task<int> MainAsync(string[] args)
    {
        using var parser = new Parser(c =>
        {
            c.HelpWriter = null;
            c.CaseInsensitiveEnumValues = true;
        });

        var parserResult = parser.ParseArguments<CliAnalyzerOptions>(args);

        var res = 0;

        var result = parserResult
          .WithParsed(options =>
            {
                res = Run(options, args);
            })
          .WithNotParsed(errs => DisplayHelp(parserResult, errs));

        await PackageService.CheckForPackageUpdateAsync().ConfigureAwait(false);

        return res;
    }

    private static int Run(CliAnalyzerOptions options, string[] args)
    {
        var sw = Stopwatch.StartNew();

        if (!options.NoLogo)
        {
            DisplayHeader(options);
        }

        SqlConnectionStringBuilder? sqlConnectionStringBuilder = null;
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            if (options.Scripts?.Count > 0)
            {
                DisplayService.Error("Cannot specify both scripts and connection string");
                return 1;
            }

            try
            {
                sqlConnectionStringBuilder = new SqlConnectionStringBuilder(options.ConnectionString);
                DisplayService.MarkupLine();
                DisplayService.MarkupLine(
                    () => DisplayService.Markup($"Connecting to database '{sqlConnectionStringBuilder.InitialCatalog}' on server '{sqlConnectionStringBuilder.DataSource}'", Decoration.Bold));
            }
            catch (Exception aex) when (aex is FormatException or KeyNotFoundException or ArgumentException)
            {
                DisplayService.Error("Invalid connection string: " + aex.Message);
                return 1;
            }
        }

        var analyzerOptions = new AnalyzerOptions
        {
            Rules = options.Rules,
            SqlVersion = options.SqlVersion,
            OutputFile = options.OutputFile != null ? new FileInfo(options.OutputFile) : null,
            ConnectionString = sqlConnectionStringBuilder,
            Format = options.Format,
        };

        bool hasBogusArgs = args.Any() && args.All(a => !a.StartsWith('-'));

        if (options.Scripts?.Count == 0 && options.ConnectionString == null && !hasBogusArgs)
        {
            analyzerOptions.Scripts = [];
            analyzerOptions.Scripts.Add(Directory.GetCurrentDirectory());
        }

        if (options.Scripts?.Count > 0)
        {
            analyzerOptions.Scripts = [.. options.Scripts];
        }

        if (options.AdditionalAnalyzers?.Count > 0)
        {
            analyzerOptions.AdditionalAnalyzers = [.. options.AdditionalAnalyzers];
        }

        var analyzerFactory = new AnalyzerFactory(analyzerOptions);

        AnalyzerResult? result;

        try
        {
            result = DisplayService.Wait(
                "Running T-SQL Analysis...",
                () => analyzerFactory.Analyze());
        }
        catch (ArgumentException aex)
        {
            DisplayService.Error(aex.Message);
            return 1;
        }

        if (result?.Result == null)
        {
            DisplayService.Error("No result from analysis");
            return 1;
        }

        var errors = result.Result.InitializationErrors
            .Concat(result.Result.SuppressionErrors)
            .Concat(result.Result.AnalysisErrors)
            .ToList();

        var hadErrors = false;

        foreach (var err in errors)
        {
            hadErrors = true;
            DisplayService.Error(err.GetOutputMessage());
        }

        if (result.ModelErrors.Count > 0)
        {
            hadErrors = true;
            foreach (var dex in result.ModelErrors)
            {
                DisplayService.Error(dex.Value.Format(dex.Key));
            }
        }

        if (result.Result.AnalysisSucceeded)
        {
            foreach (var err in result.Result.Problems)
            {
                hadErrors = true;
                var warning = err.GetOutputMessage(analyzerOptions.Rules);

                if (options.NoLogo)
                {
                    Console.WriteLine(warning);
                }
                else
                {
                    DisplayService.MarkupLine(
                    () => DisplayService.Markup("warning:", Color.Yellow),
                    () => DisplayService.Markup(warning, Decoration.None));
                }
            }

            if (analyzerOptions.OutputFile != null)
            {
                DisplayService.MarkupLine();
                DisplayService.MarkupLine(
                    () => DisplayService.Markup($"Writing report to '{analyzerOptions.OutputFile.FullName}'", Decoration.Bold));
            }

            if (analyzerOptions.Format)
            {
                DisplayService.MarkupLine();
                DisplayService.MarkupLine(
                    () => DisplayService.Markup($"PREVIEW: Formatting {result.FormattedFiles.Count} files", Decoration.Bold));

                foreach (var file in result.FormattedFiles)
                {
                    DisplayService.MarkupLine(
                        () => DisplayService.Markup($"PREVIEW: Formatted '{file}'", Decoration.Bold));
                }
            }

            DisplayService.MarkupLine();
            if (result.FileCount > 0)
            {
                DisplayService.MarkupLine(
                    () => DisplayService.Markup($"Analyzed {result.FileCount} files in {sw.Elapsed.TotalSeconds:N3} seconds using '{result.Analyzers}'. {result.Result.Problems.Count} problems found.", Decoration.Bold));
            }
            else
            {
                DisplayService.MarkupLine(
                    () => DisplayService.Markup($"Analysis completed in {sw.Elapsed.TotalSeconds:N3} seconds. {result.Result.Problems.Count} problems found.", Decoration.Bold));
            }

            return hadErrors ? 1 : 0;
        }

        return 1;
    }

    private static void DisplayHeader(CliAnalyzerOptions options)
    {
        DisplayService.Title("T-SQL Analyze");
        DisplayService.MarkupLine(
            $"T-SQL Analyzer CLI {PackageService.CurrentPackageVersion()}",
            Color.Cyan1);
        DisplayService.MarkupLine("https://github.com/ErikEJ/SqlServer.Rules", Color.Blue, DisplayService.Link);
        DisplayService.MarkupLine();
    }

    private static int DisplayHelp(ParserResult<CliAnalyzerOptions> parserResult, IEnumerable<Error> errors)
    {
        Console.WriteLine(HelpText.AutoBuild(parserResult, h =>
        {
            h.AddPostOptionsLine("SAMPLES:");
            h.AddPostOptionsLine(string.Empty);
            h.AddPostOptionsLine("# Analyze all scripts in current folder and sub-folders");
            h.AddPostOptionsLine("tsqlanalyze");
            h.AddPostOptionsLine(string.Empty);
            h.AddPostOptionsLine("## Analyze a single script");
            h.AddPostOptionsLine("tsqlanalyze -i C:\\scripts\\sproc.sql");
            h.AddPostOptionsLine(string.Empty);
            h.AddPostOptionsLine("## Analyze scripts in a folder");
            h.AddPostOptionsLine("tsqlanalyze -i \"c:\\database scripts\"");
            h.AddPostOptionsLine(string.Empty);
            h.AddPostOptionsLine("## Analyze scripts in folders with a wildcard path and a full folder path");
            h.AddPostOptionsLine("tsqlanalyze -i c:\\database_scripts\\sp_*.sql \"c:\\old scripts\"");
            h.AddPostOptionsLine(string.Empty);
            h.AddPostOptionsLine("## Analyze a script with a rule filter");
            h.AddPostOptionsLine("tsqlanalyze -i C:\\scripts\\sproc.sql -r Rules:-SqlServer.Rules.SRD0004");
            h.AddPostOptionsLine(string.Empty);
            h.AddPostOptionsLine("## Analyze a script for a specific SQL Server version");
            h.AddPostOptionsLine("tsqlanalyze -i C:\\scripts\\sproc.sql -s SqlAzure");
            h.AddPostOptionsLine(string.Empty);
            h.AddPostOptionsLine("## Analyze a .dacpac and save results to an xml file");
            h.AddPostOptionsLine("tsqlanalyze -i C:\\scripts\\Chinook.dacpac -o result.xml");
            h.AddPostOptionsLine(string.Empty);
            h.AddPostOptionsLine("## Analyze a single file, and reformat (PREVIEW)");
            h.AddPostOptionsLine("tsqlanalyze -i C:\\scripts\\sproc.sql -f");
            h.AddPostOptionsLine(string.Empty);
            h.AddPostOptionsLine("## Analyze a .zip file with .sql files");
            h.AddPostOptionsLine("tsqlanalyze -i C:\\scripts\\Fabric.zip");
            h.AddPostOptionsLine(string.Empty);
            h.AddPostOptionsLine("## Analyze a live database");
            h.AddPostOptionsLine("tsqlanalyze -c \"Data Source=.\\SQLEXPRESS;Initial Catalog=Chinook;Integrated Security=True;Encrypt=false\"");

            return h;
        }));

        return 1;
    }
}
