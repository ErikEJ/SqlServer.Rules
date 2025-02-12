using System.Diagnostics;
using System.Globalization;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Spectre.Console;
using SqlAnalyzerCli.Services;

namespace ErikEJ.SqlAnalyzer;

internal sealed class AnalyzerFactory
{
    public int Create(AnalyzerOptions request)
    {
        // TODO Collect all script paths and validate that they exist
        // if no files found, return with error message
        var fileName = request.Scripts.First();

        var sw = Stopwatch.StartNew();

        using var model = new TSqlModel(SqlServerVersion.Sql160, new TSqlModelOptions());

        model.AddOrUpdateObjects(File.ReadAllText(fileName), fileName, new TSqlObjectOptions());

        var factory = new CodeAnalysisServiceFactory();
        var service = factory.CreateAnalysisService(model);

        // TODO supress and ignore rules - see PackageAnalyzer implementation
        sw.Stop();
        SendNotification($"Loading files completed in: {sw.Elapsed.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)}", Color.Default);

        sw = Stopwatch.StartNew();

        // process non-suppressed rules
        var result = service.Analyze(model);

        foreach (var err in result.InitializationErrors)
        {
            DisplayService.Error(err.Message);
        }

        foreach (var err in result.SuppressionErrors)
        {
            DisplayService.Error(err.Message);
        }

        foreach (var err in result.AnalysisErrors)
        {
            DisplayService.Error(err.Message);
        }

        if (result.AnalysisSucceeded)
        {
            foreach (var err in result.Problems)
            {
                // TODO Add support for error wild cards parameter here
                SendNotification(err.GetOutputMessage([]), Color.Yellow);
            }
        }

        SendNotification($"Analysis completed in: {sw.Elapsed.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)}", Color.Default);
        return 0;
    }

    private void SendNotification(string message, Color color)
    {
        DisplayService.MarkupLine(message, color);
    }
}