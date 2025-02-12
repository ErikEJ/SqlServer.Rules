using System.Diagnostics;
using CommandLine;
using CommandLine.Text;
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

        var parserResult = parser.ParseArguments<AnalyzerOptions>(args);

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

    private static int Run(AnalyzerOptions options)
    {
        DisplayHeader(options);
        var analyzerFactory = new AnalyzerFactory();
        return analyzerFactory.Create(options);
    }

    private static void DisplayHeader(AnalyzerOptions options)
    {
        DisplayService.Title("T-SQL Analyze");
        DisplayService.MarkupLine(
            $"T-SQL Analyze CLI {PackageService.CurrentPackageVersion()}",
            Color.Cyan1);
        DisplayService.MarkupLine("https://github.com/ErikEJ/SqlServer.Rules", Color.Blue, DisplayService.Link);
        DisplayService.MarkupLine();
    }

    private static int DisplayHelp(ParserResult<AnalyzerOptions> parserResult, IEnumerable<Error> errors)
    {
        Console.WriteLine(HelpText.AutoBuild(parserResult, h =>
        {
            h.AddPostOptionsLine("SAMPLES:");
            h.AddPostOptionsLine(@"  tsqlanalyze -i C:\scripts\sproc.sql");
            h.AddPostOptionsLine(@"  tsqlanalyze -i file_one.sql file_two.sql ""c:\database scripts""");
            h.AddPostOptionsLine(@"  tsqlanalyze -i c:\database_scripts\sp_ *.sql");
            h.AddPostOptionsLine(@"  tsqlanalyze -i C:\scripts\sproc.sql - ""-SqlServer.Rules.SRD0004"" -s SqlAzure");
            return h;
        }));

        return 1;
    }
}
