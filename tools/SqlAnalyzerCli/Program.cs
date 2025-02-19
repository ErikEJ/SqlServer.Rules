using System.Diagnostics;
using CommandLine;
using CommandLine.Text;
using ErikEJ.DacFX.TSQLAnalyzer;
using ErikEJ.DacFX.TSQLAnalyzer.Extensions;
using Spectre.Console;
using SqlAnalyzerCli.Services;

namespace ErikEJ.SqlAnalyzer;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            return await MainAsync(args).ConfigureAwait(false);
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
                res = Run(options);
            })
          .WithNotParsed(errs => DisplayHelp(parserResult, errs));

        await PackageService.CheckForPackageUpdateAsync().ConfigureAwait(false);

        return res;
    }

    private static int Run(CliAnalyzerOptions options)
    {
        if (!options.NoLogo)
        {
            DisplayHeader(options);
        }

        var analyzerOptions = new AnalyzerOptions
        {
            Rules = options.Rules,
            SqlVersion = options.SqlVersion,
        };

        analyzerOptions.Scripts.AddRange(options.Scripts);

        var analyzerFactory = new AnalyzerFactory(analyzerOptions);

        AnalyzerResult result;

        try
        {
            result = analyzerFactory.Analyze();
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

        foreach (var err in result.Result.InitializationErrors)
        {
            DisplayService.Error(err.Message);
        }

        foreach (var err in result.Result.SuppressionErrors)
        {
            DisplayService.Error(err.Message);
        }

        foreach (var err in result.Result.AnalysisErrors)
        {
            DisplayService.Error(err.Message);
        }

        if (result.ModelErrors.Count > 0)
        {
            foreach (var dex in result.ModelErrors)
            {
                DisplayService.Error(dex.Value.Format(dex.Key));
            }
        }

        if (result.Result.AnalysisSucceeded)
        {
            foreach (var err in result.Result.Problems)
            {
                var warning = err.GetOutputMessage(analyzerOptions.Rules);

                warning = warning
                    .Replace("[", "[[", StringComparison.OrdinalIgnoreCase)
                    .Replace("]", "]]", StringComparison.OrdinalIgnoreCase);

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
        }

        return 0;
    }

    private static void DisplayHeader(CliAnalyzerOptions options)
    {
        DisplayService.Title("T-SQL Analyze");
        DisplayService.MarkupLine(
            $"T-SQL Analyze CLI {PackageService.CurrentPackageVersion()}",
            Color.Cyan1);
        DisplayService.MarkupLine("https://github.com/ErikEJ/SqlServer.Rules", Color.Blue, DisplayService.Link);
        DisplayService.MarkupLine();
    }

    private static int DisplayHelp(ParserResult<CliAnalyzerOptions> parserResult, IEnumerable<Error> errors)
    {
        Console.WriteLine(HelpText.AutoBuild(parserResult, h =>
        {
            h.AddPostOptionsLine("SAMPLES:");
            h.AddPostOptionsLine(@"  tsqlanalyze -i C:\scripts\sproc.sql");
            h.AddPostOptionsLine(@"  tsqlanalyze -i file_one.sql file_two.sql ""c:\database scripts""");
            h.AddPostOptionsLine(@"  tsqlanalyze -i c:\database_scripts\sp_*.sql");
            h.AddPostOptionsLine(@"  tsqlanalyze -i C:\scripts\sproc.sql -r Rules:-SqlServer.Rules.SRD0004 -s SqlAzure");
            return h;
        }));

        return 1;
    }
}
