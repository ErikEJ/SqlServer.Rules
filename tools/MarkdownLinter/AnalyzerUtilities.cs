namespace SqlAnalyzer;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.Languages;
using Microsoft.VisualStudio.RpcContracts.DiagnosticManagement;
using Microsoft.VisualStudio.Threading;

#pragma warning disable VSEXTPREVIEW_SETTINGS // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

/// <summary>
/// Helper class for running linter on a string or file.
/// </summary>
internal class AnalyzerUtilities
{
    // Regex to parse T-SQL analyzer CLI output format:
    // filename(line,column): ruleid : description
    private static readonly Regex SqlAnalyzerOutputRegex = new(
        @"^(?<filename>.+?)\((?<line>\d+),(?<column>\d+)\):\s*(?<ruleid>[^:]+)\s*:\s*(?<description>.*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Runs SQL analyzer on a file uri and returns diagnostic entries.
    /// </summary>
    /// <param name="fileUri">File uri to run SQL analyzer on.</param>
    /// <param name="cancellationToken">Cancellation token to monitor.</param>
    /// <returns>an enumeration of <see cref="DocumentDiagnostic"/> entries for warnings in the SQL file.</returns>
    public async Task<IEnumerable<DocumentDiagnostic>> RunAnalyzerOnFileAsync(Uri fileUri, CancellationToken cancellationToken)
    {
        using var linter = new Process();
        var lineQueue = new AsyncQueue<string>();

        string args = "/c \"tsqlanalyze -n -i" +
            $" \"{fileUri.LocalPath}\"\"";

        linter.StartInfo = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            Arguments = args,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        linter.EnableRaisingEvents = true;
        linter.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
        {
            if (e.Data is not null)
            {
                lineQueue.Enqueue(e.Data);
            }
            else
            {
                lineQueue.Complete();
            }
        });

        try
        {
            linter.Start();
            linter.BeginOutputReadLine();
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(message: ex.Message, innerException: ex);
        }

        var markdownDiagnostics = await ProcessLinterQueueAsync(lineQueue);
        return CreateDocumentDiagnosticsForClosedDocument(fileUri, markdownDiagnostics);
    }

    /// <summary>
    /// Runs markdown linter on a given text document and returns diagnostic entries.
    /// </summary>
    /// <param name="textDocument">Document to run markdown linter on.</param>
    /// <param name="cancellationToken">Cancellation token to monitor.</param>
    /// <returns>an enumeration of <see cref="DocumentDiagnostic"/> entries for warnings in the markdown file.</returns>
    public async Task<IEnumerable<DocumentDiagnostic>> RunLinterOnDocumentAsync(ITextDocumentSnapshot textDocument, CancellationToken cancellationToken)
    {
        using var linter = new Process();
        var lineQueue = new AsyncQueue<string>();

        var content = textDocument.Text.CopyToString();

        ////var snapshot = await this.settingsObserver.GetSnapshotAsync(cancellationToken);
        ////string disabledRules = snapshot.DisabledRules.ValueOrDefault(string.Empty);

        ////string args = "/k \"npx markdownlint-cli --stdin" +
        ////    (disabledRules.Length > 0 ? $" --disable {disabledRules}" : string.Empty) + "\"";

        ////linter.StartInfo = new ProcessStartInfo()
        ////{
        ////    FileName = "cmd.exe",
        ////    Arguments = args,
        ////    RedirectStandardError = true,
        ////    RedirectStandardInput = true,
        ////    UseShellExecute = false,
        ////    CreateNoWindow = true,
        ////};

        ////linter.EnableRaisingEvents = true;
        ////linter.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
        ////{
        ////    if (e.Data is not null)
        ////    {
        ////        lineQueue.Enqueue(e.Data);
        ////    }
        ////    else
        ////    {
        ////        lineQueue.Complete();
        ////    }
        ////});

        ////try
        ////{
        ////    linter.Start();
        ////    linter.BeginErrorReadLine();
        ////    linter.StandardInput.AutoFlush = true;
        ////    await linter.StandardInput.WriteAsync(content);

        ////    linter.StandardInput.Close();
        ////}
        ////catch (Win32Exception ex)
        ////{
        ////    throw new InvalidOperationException(message: ex.Message, innerException: ex);
        ////}

        var markdownDiagnostics = await ProcessLinterQueueAsync(lineQueue);
        return CreateDocumentDiagnosticsForOpenDocument(textDocument, markdownDiagnostics);
    }

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
                ProviderName = "T-SQL Analyzer",
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
                ProviderName = "T-SQL Analyzer",
            };
        }
    }

    private static async Task<IEnumerable<SqlAnalyzerDiagnosticInfo>> ProcessLinterQueueAsync(AsyncQueue<string> lineQueue)
    {
        Requires.NotNull(lineQueue, nameof(lineQueue));

        List<SqlAnalyzerDiagnosticInfo> diagnostics = new List<SqlAnalyzerDiagnosticInfo>();

        while (!(lineQueue.IsCompleted && lineQueue.IsEmpty))
        {
            string? line;
            try
            {
                line = await lineQueue.DequeueAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var diagnostic = line is not null ? GetDiagnosticFromLinterOutput(line) : null;
            if (diagnostic is not null)
            {
                diagnostics.Add(diagnostic);
            }
            else
            {
                // Something went wrong so break and return the current set.
                break;
            }
        }

        return diagnostics;
    }

    private static SqlAnalyzerDiagnosticInfo? GetDiagnosticFromLinterOutput(string outputLine)
    {
        Requires.NotNull(outputLine, nameof(outputLine));

        var parsed = ParseSqlAnalyzerOutput(outputLine);

        if (parsed is null)
        {
            // The output line does not match the expected format.
            return null;
        }

        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(5,9): Smells.SML005 : Avoid use of 'Select *'. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/CodeSmells/SML005.md)
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(1,18): Smells.SML030 : Include SET NOCOUNT ON inside stored procedures. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/CodeSmells/SML030.md)
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(5,9): Microsoft.Rules.Data.SR0001 : The shape of the result set produced by a SELECT * statement will change if the underlying table or view structure changes. (https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0001-avoid-select--in-stored-procedures-views-and-table-valued-functions)
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(1,24): Microsoft.Rules.Data.SR0016 : Stored procedure(sp_Procedure1) includes sp_ prefix in its name. (https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-naming-issues#sr0016-avoid-using-sp_-as-a-prefix-for-stored-procedures)
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(5,9): SqlServer.Rules.SRD0006 : Avoid using SELECT *. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/Design/SRD0006.md)
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(2,2): SqlServer.Rules.SRD0016 : Input parameter '@param1' is never used. Consider removing the parameter or using it. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/Design/SRD0016.md)
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(3,2): SqlServer.Rules.SRD0016 : Input parameter '@param2' is never used. Consider removing the parameter or using it. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/Design/SRD0016.md)
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(1,1): SqlServer.Rules.SRD0067 : Capitalize the keyword 'int' for enhanced readability. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/Design/SRD0067.md)
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(1,1): SqlServer.Rules.SRD0067 : Capitalize the keyword 'int' for enhanced readability. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/Design/SRD0067.md)
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(1,1): SqlServer.Rules.SRD0068 : Query statements should finish with a semicolon - ';'.
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(5,2): SqlServer.Rules.SRD0068 : Query statements should finish with a semicolon - ';'.
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(6,1): SqlServer.Rules.SRD0068 : Query statements should finish with a semicolon - ';'.
        //C:\dev\GitHub\EFCorePowerTools\test\ScaffoldingTester\Database\dbo\Procedures\Procedure1.sql(1,1): SqlServer.Rules.SRP0005 : SET NOCOUNT ON is recommended to be enabled in stored procedures and triggers. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/Performance/SRP0005.md)

        // TODO - Massage the description and ruleid to match the expected format.
        return new SqlAnalyzerDiagnosticInfo(
            range: new Microsoft.VisualStudio.RpcContracts.Utilities.Range(startLine: parsed.Value.Line - 1, startColumn: parsed.Value.Column - 1),
            message: parsed.Value.Description,
            errorCode: parsed.Value.RuleId);
    }

        /// <summary>
    /// Parses a SQL analyzer output line and extracts its components.
    /// </summary>
    /// <param name="outputLine">The output line to parse</param>
    /// <returns>A tuple containing the parsed components, or null if parsing fails</returns>
    public static (string Filename, int Line, int Column, string RuleId, string Description)? ParseSqlAnalyzerOutput(string outputLine)
    {
        if (string.IsNullOrWhiteSpace(outputLine))
        {
            return null;
        }

        var match = SqlAnalyzerOutputRegex.Match(outputLine);
        if (!match.Success)
        {
            return null;
        }

        if (!int.TryParse(match.Groups["line"].Value, out int line) ||
            !int.TryParse(match.Groups["column"].Value, out int column))
        {
            return null;
        }

        return (
            Filename: match.Groups["filename"].Value,
            Line: line,
            Column: column,
            RuleId: match.Groups["ruleid"].Value.Trim(),
            Description: match.Groups["description"].Value.Trim()
        );
    }
}
