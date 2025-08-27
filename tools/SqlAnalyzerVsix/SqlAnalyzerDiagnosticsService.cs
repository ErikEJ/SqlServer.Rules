namespace SqlAnalyzer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Documents;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.Helpers;
using Microsoft.VisualStudio.ProjectSystem.Query;
using Microsoft.VisualStudio.Threading;

/// <summary>
/// An internal service shared across extension components via dependency injection. The service provides
/// a central mechanism to manage SQL diagnostics for documents and can be called from various parts such as
/// commands or editor listeners.
/// </summary>
/// <remarks>For a sample ingestion of this service, see <see cref="TextViewEventListener"/> constructor.</remarks>
#pragma warning disable VSEXTPREVIEW_OUTPUTWINDOW // Type is for evaluation purposes only and is subject to change or removal in future updates.
internal class SqlAnalyzerDiagnosticsService : DisposableObject
{
#pragma warning disable CA2213 // Disposable fields should be disposed, object now owned by this instance.
    private readonly VisualStudioExtensibility extensibility;
#pragma warning restore CA2213 // Disposable fields should be disposed
    private readonly Dictionary<Uri, CancellationTokenSource> documentCancellationTokens;
    private readonly Task initializationTask;
    private readonly AnalyzerUtilities analyzerUtilities;
    private OutputChannel? outputChannel;
    private DiagnosticsReporter? diagnosticsReporter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlAnalyzerDiagnosticsService"/> class.
    /// </summary>
    /// <param name="extensibility">Extensibility object.</param>
    /// <param name="analyzerUtilities">Service for running the analyzer on SQL files.</param>
    public SqlAnalyzerDiagnosticsService(VisualStudioExtensibility extensibility, AnalyzerUtilities analyzerUtilities)
    {
        this.extensibility = Requires.NotNull(extensibility, nameof(extensibility));
        this.documentCancellationTokens = new Dictionary<Uri, CancellationTokenSource>();
        this.analyzerUtilities = Requires.NotNull(analyzerUtilities, nameof(analyzerUtilities));
        this.initializationTask = Task.Run(this.InitializeAsync);
    }

    /// <summary>
    /// Processes the specified file for SQL errors and reports to the error list.
    /// </summary>
    /// <param name="documentUri">Document uri to read the contents from.</param>
    /// <param name="cancellationToken">Cancellation token to monitor.</param>
    /// <returns>Task indicating completion of reporting markdown errors to error list.</returns>
    public async Task ProcessFileAsync(Uri documentUri, CancellationToken cancellationToken)
    {
        CancellationTokenSource newCts = new CancellationTokenSource();
        lock (this.documentCancellationTokens)
        {
            if (this.documentCancellationTokens.TryGetValue(documentUri, out var cts))
            {
                _ = cts.CancelAsync();
            }

            this.documentCancellationTokens[documentUri] = newCts;
        }

        // Wait for 1 second to see if any other changes are being sent.
        await Task.Delay(1000, cancellationToken);

        if (newCts.IsCancellationRequested)
        {
            return;
        }

        try
        {
            var runProperties = await this.IsInSqlProjAsync(documentUri.LocalPath, cancellationToken);

            var diagnostics = await this.analyzerUtilities.RunAnalyzerOnFileAsync(documentUri, runProperties.Rules, runProperties.SqlVersion, cancellationToken);

            await this.diagnosticsReporter!.ClearDiagnosticsAsync(documentUri, cancellationToken);
            await this.diagnosticsReporter!.ReportDiagnosticsAsync(diagnostics, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            if (this.outputChannel is not null)
            {
                await this.outputChannel.WriteLineAsync(Strings.MissingLinterError);
            }
        }
    }

    /// <summary>
    /// Processes the current version <see cref="ITextViewSnapshot"/> instance for SQL errors and reports to the error list.
    /// </summary>
    /// <param name="textViewSnapshot">Text View instance to read the contents from.</param>
    /// <param name="cancellationToken">Cancellation token to monitor.</param>
    /// <returns>Task indicating completion of reporting SQL errors to error list.</returns>
    public async Task ProcessTextViewAsync(ITextViewSnapshot textViewSnapshot, CancellationToken cancellationToken)
    {
        CancellationTokenSource newCts = new CancellationTokenSource();
        lock (this.documentCancellationTokens)
        {
            if (this.documentCancellationTokens.TryGetValue(textViewSnapshot.Document.Uri, out var cts))
            {
                _ = cts.CancelAsync();
            }

            this.documentCancellationTokens[textViewSnapshot.Document.Uri] = newCts;
        }

        var runProperties = await this.IsInSqlProjAsync(textViewSnapshot.Uri.LocalPath, cancellationToken);
        if (runProperties.Run)
        {
            await this.ProcessDocumentAsync(textViewSnapshot.Document, runProperties.Rules, runProperties.SqlVersion, cancellationToken.CombineWith(newCts.Token).Token);
        }
    }

    /// <summary>
    /// Clears any of the existing entries for the specified document uri.
    /// </summary>
    /// <param name="documentUri">Document uri to clear SQL error entries for.</param>
    /// <param name="cancellationToken">Cancellation token to monitor.</param>
    /// <returns>Task indicating completion.</returns>
    public async Task ClearEntriesForDocumentAsync(Uri documentUri, CancellationToken cancellationToken)
    {
        lock (this.documentCancellationTokens)
        {
            if (this.documentCancellationTokens.TryGetValue(documentUri, out var cts))
            {
                _ = cts.CancelAsync();
                this.documentCancellationTokens.Remove(documentUri);
            }
        }

        await this.diagnosticsReporter!.ClearDiagnosticsAsync(documentUri, cancellationToken);
    }

    /// <inheritdoc />
    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        if (isDisposing)
        {
            this.outputChannel?.Dispose();
            this.diagnosticsReporter?.Dispose();
        }
    }

    private async Task ProcessDocumentAsync(ITextDocumentSnapshot documentSnapshot, string? rules, string? sqlVersion, CancellationToken cancellationToken)
    {
        // Wait for 1 second to see if any other changes are being sent.
        await Task.Delay(1000, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            var diagnostics = await this.analyzerUtilities.RunAnalyzerOnDocumentAsync(documentSnapshot, rules, sqlVersion, cancellationToken);

            await this.diagnosticsReporter!.ClearDiagnosticsAsync(documentSnapshot, cancellationToken);
            await this.diagnosticsReporter!.ReportDiagnosticsAsync(diagnostics, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            if (this.outputChannel is not null)
            {
                await this.outputChannel.WriteLineAsync(Strings.MissingLinterError);
            }
        }
    }

    private async Task InitializeAsync()
    {
        this.outputChannel = await this.extensibility.Views().Output.CreateOutputChannelAsync(Strings.MarkdownLinterWindowName, default);
        this.diagnosticsReporter = this.extensibility.Languages().GetDiagnosticsReporter(nameof(SqlAnalyzerExtension));
        Assumes.NotNull(this.diagnosticsReporter);
    }

    private async Task<(bool Run, string? Rules, string? SqlVersion)> IsInSqlProjAsync(string path, CancellationToken cancellationToken)
    {
        var workspace = this.extensibility.Workspaces();

#pragma warning disable VSEXTPREVIEW_PROJECTQUERY_PROPERTIES_BUILDPROPERTIES // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        IQueryResults<IProjectSnapshot> projects = await workspace.QueryProjectsAsync(
            project => project
                .WithRequired(project => project.FilesByPath(path))
                .With(p => p.ActiveConfigurations
                    .With(c => c.PropertiesByName(PropertySourceType.ProjectFile, "RunSqlCodeAnalysis", "CodeAnalysisRules", "AnalyzerCodeAnalysisRules", "SqlServerVersion", "DSP"))),
            cancellationToken);

        var runAnalyzer = projects.Any(p => p.ActiveConfigurations.Any(c =>
            c.Properties.Any(prop =>
                prop.Name.Equals("RunSqlCodeAnalysis", StringComparison.OrdinalIgnoreCase)
                && prop.Value != null
                && prop.Value.Equals("true", StringComparison.OrdinalIgnoreCase))));

        var rules = projects.SelectMany(p => p.ActiveConfigurations)
            .SelectMany(c => c.Properties)
            .FirstOrDefault(prop => prop.Name.Equals("CodeAnalysisRules", StringComparison.OrdinalIgnoreCase))?.Value;

        var legacyRules = projects.SelectMany(p => p.ActiveConfigurations)
            .SelectMany(c => c.Properties)
            .FirstOrDefault(prop => prop.Name.Equals("AnalyzerCodeAnalysisRules", StringComparison.OrdinalIgnoreCase))?.Value;

        var sqlVersion = projects.SelectMany(p => p.ActiveConfigurations)
            .SelectMany(c => c.Properties)
            .FirstOrDefault(prop => prop.Name.Equals("SqlServerVersion", StringComparison.OrdinalIgnoreCase))?.Value;

        var dsp = projects.SelectMany(p => p.ActiveConfigurations)
            .SelectMany(c => c.Properties)
            .FirstOrDefault(prop => prop.Name.Equals("DSP", StringComparison.OrdinalIgnoreCase))?.Value;

        string? dspVersion = null;

        // https://learn.microsoft.com/en-us/sql/tools/sql-database-projects/concepts/target-platform?view=sql-server-ver17&pivots=sq1-visual-studio#sql-project-file-sample-and-syntax
        if (!string.IsNullOrEmpty(dsp))
        {
            // Microsoft.Data.Tools.Schema.Sql.Sql160DatabaseSchemaProvider (SQL Server 2022)
            // Microsoft.Data.Tools.Schema.Sql.SqlAzureV12DatabaseSchemaProvider (Azure SQL Database)
            dspVersion = dsp.Replace("Microsoft.Data.Tools.Schema.Sql.", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("DatabaseSchemaProvider", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("V12", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim();
        }

#pragma warning restore VSEXTPREVIEW_PROJECTQUERY_PROPERTIES_BUILDPROPERTIES // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        runAnalyzer = runAnalyzer && projects.Count() == 1;

        return (runAnalyzer, legacyRules ?? rules, dspVersion ?? sqlVersion);
    }
}
#pragma warning restore VSEXTPREVIEW_OUTPUTWINDOW // Type is for evaluation purposes only and is subject to change or removal in future updates.
