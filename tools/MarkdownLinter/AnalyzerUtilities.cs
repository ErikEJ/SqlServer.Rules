namespace MarkdownLinter;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ErikEJ.DacFX.TSQLAnalyzer;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.Languages;
using Microsoft.VisualStudio.RpcContracts.DiagnosticManagement;

#pragma warning disable VSEXTPREVIEW_SETTINGS // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

/// <summary>
/// Helper class for running linter on a string or file.
/// </summary>
internal sealed class AnalyzerUtilities
{
    public async Task<IEnumerable<DocumentDiagnostic>> RunAnalyzerOnFileAsync(Uri fileUri, CancellationToken cancellationToken)
    {
        var files = new List<string> { fileUri.LocalPath };

        var analyzerOptions = new AnalyzerOptions();

        analyzerOptions.Scripts!.AddRange(files);

        var analyzerFactory = new AnalyzerFactory(analyzerOptions);

        AnalyzerResult result;

        try
        {
            result = await Task.Run(analyzerFactory.Analyze);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(message: ex.Message, innerException: ex);
        }

        if (result?.Result == null)
        {
            return CreateDocumentDiagnosticsForClosedDocument(
                fileUri,
                [
                    new(
                        range: new Microsoft.VisualStudio.RpcContracts.Utilities.Range(0, 0),
                        message: "No analysis result available.",
                        errorCode: "SQLLINT001"),
                ]);
        }

        ////foreach (var err in result.Result.InitializationErrors)
        ////{
        ////    Console.WriteLine(err.Message);
        ////}

        ////foreach (var err in result.Result.SuppressionErrors)
        ////{
        ////    Console.WriteLine(err.Message);
        ////}

        ////foreach (var err in result.Result.AnalysisErrors)
        ////{
        ////    Console.WriteLine(err.Message);
        ////}

        ////if (result.ModelErrors.Count > 0)
        ////{
        ////    foreach (var dex in result.ModelErrors)
        ////    {
        ////        Console.WriteLine(dex.Value.Format(dex.Key));
        ////    }
        ////}

        if (result.Result.AnalysisSucceeded)
        {
            var markdownDiagnostics = ProcessProblems(result.Result.Problems);
            return CreateDocumentDiagnosticsForClosedDocument(fileUri, markdownDiagnostics);
        }

        return CreateDocumentDiagnosticsForClosedDocument(
                fileUri,
                [
                    new(
                        range: new Microsoft.VisualStudio.RpcContracts.Utilities.Range(0, 0),
                        message: "Analyzer failed.",
                        errorCode: "SQLLINT001"),
                ]);
    }

    ///// <summary>
    ///// Runs markdown linter on a given text document and returns diagnostic entries.
    ///// </summary>
    ///// <param name="textDocument">Document to run markdown linter on.</param>
    ///// <param name="cancellationToken">Cancellation token to monitor.</param>
    ///// <returns>an enumeration of <see cref="DocumentDiagnostic"/> entries for warnings in the markdown file.</returns>
    //public async Task<IEnumerable<DocumentDiagnostic>> RunLinterOnDocumentAsync(ITextDocumentSnapshot textDocument, CancellationToken cancellationToken)
    //{
    //    using var linter = new Process();
    //    var lineQueue = new AsyncQueue<string>();

    //    var content = textDocument.Text.CopyToString();

    //    linter.StartInfo = new ProcessStartInfo()
    //    {
    //        FileName = "cmd.exe",
    //        Arguments = "args",
    //        RedirectStandardError = true,
    //        RedirectStandardInput = true,
    //        UseShellExecute = false,
    //        CreateNoWindow = true,
    //    };

    //    linter.EnableRaisingEvents = true;
    //    linter.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
    //    {
    //        if (e.Data is not null)
    //        {
    //            lineQueue.Enqueue(e.Data);
    //        }
    //        else
    //        {
    //            lineQueue.Complete();
    //        }
    //    });

    //    try
    //    {
    //        linter.Start();
    //        linter.BeginErrorReadLine();
    //        linter.StandardInput.AutoFlush = true;
    //        await linter.StandardInput.WriteAsync(content);

    //        linter.StandardInput.Close();
    //    }
    //    catch (Win32Exception ex)
    //    {
    //        throw new InvalidOperationException(message: ex.Message, innerException: ex);
    //    }

    //    var markdownDiagnostics = await ProcessLinterQueueAsync(lineQueue);
    //    return CreateDocumentDiagnosticsForOpenDocument(textDocument, markdownDiagnostics);
    //}

    private static IEnumerable<DocumentDiagnostic> CreateDocumentDiagnosticsForOpenDocument(ITextDocumentSnapshot document, IEnumerable<SqlAnalyzerDiagnosticInfo> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            var startindex = document.Lines[diagnostic.Range.StartLine].Text.Start.Offset;
            if (diagnostic.Range.StartColumn >= 0)
            {
                startindex += diagnostic.Range.StartColumn;
            }

            var endIndex = document.Lines[diagnostic.Range.EndLine].Text.Start.Offset;
            if (diagnostic.Range.EndColumn >= 0)
            {
                endIndex += diagnostic.Range.EndColumn;
            }

            yield return new DocumentDiagnostic(new TextRange(document, startindex, endIndex - startindex), diagnostic.Message)
            {
                ErrorCode = diagnostic.ErrorCode,
                Severity = DiagnosticSeverity.Warning,
                ProviderName = "Markdown Linter",
            };
        }
    }

    private static IEnumerable<DocumentDiagnostic> CreateDocumentDiagnosticsForClosedDocument(Uri fileUri, IEnumerable<SqlAnalyzerDiagnosticInfo> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            yield return new DocumentDiagnostic(fileUri, diagnostic.Range, diagnostic.Message)
            {
                ErrorCode = diagnostic.ErrorCode,
                Severity = DiagnosticSeverity.Warning,
                ProviderName = "Markdown Linter",
            };
        }
    }

    private static IEnumerable<SqlAnalyzerDiagnosticInfo> ProcessProblems(ReadOnlyCollection<SqlRuleProblem> problems)
    {
        List<SqlAnalyzerDiagnosticInfo> diagnostics = new List<SqlAnalyzerDiagnosticInfo>();

        foreach (var problem in problems)
        {
            var diagnostic = new SqlAnalyzerDiagnosticInfo(
                range: new Microsoft.VisualStudio.RpcContracts.Utilities.Range(problem.StartLine, problem.StartColumn),
                message: problem.Description,
                errorCode: problem.RuleId);
            diagnostics.Add(diagnostic);
        }

        return diagnostics;
    }
}
