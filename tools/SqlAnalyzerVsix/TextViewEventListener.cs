namespace SqlAnalyzer;

using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;

/// <summary>
/// Listener for text view lifetime events to start linter on new documents or changed documents.
/// </summary>
[VisualStudioContribution]
internal class TextViewEventListener : ExtensionPart, ITextViewOpenClosedListener, ITextViewChangedListener
{
#pragma warning disable CA2213 // This is an extension scoped service.
    private readonly SqlAnalyzerDiagnosticsService diagnosticsProvider;
#pragma warning restore CA2213

    /// <summary>
    /// Initializes a new instance of the <see cref="TextViewEventListener"/> class.
    /// </summary>
    /// <param name="extension">Extension instance.</param>
    /// <param name="extensibility">Extensibility object.</param>
    /// <param name="diagnosticsProvider">Local diagnostics provider service instance.</param>
    public TextViewEventListener(SqlAnalyzerExtension extension, VisualStudioExtensibility extensibility, SqlAnalyzerDiagnosticsService diagnosticsProvider)
        : base(extension, extensibility)
    {
        this.diagnosticsProvider = Requires.NotNull(diagnosticsProvider, nameof(diagnosticsProvider));
    }

    /// <inheritdoc/>
    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
    {
        AppliesTo =
        [
            DocumentFilter.FromGlobPattern("**/*.sql", true),
        ],
    };

    /// <inheritdoc />
    public async Task TextViewChangedAsync(TextViewChangedArgs args, CancellationToken cancellationToken)
    {
        string? path = args.AfterTextView.FilePath;

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        await this.diagnosticsProvider.ProcessTextViewAsync(args.AfterTextView, cancellationToken);
    }

    /// <inheritdoc />
    public async Task TextViewClosedAsync(ITextViewSnapshot textViewSnapshot, CancellationToken cancellationToken)
    {
        await this.diagnosticsProvider.ClearEntriesForDocumentAsync(textViewSnapshot.Document.Uri, cancellationToken);
    }

    /// <inheritdoc />
    public async Task TextViewOpenedAsync(ITextViewSnapshot textViewSnapshot, CancellationToken cancellationToken)
    {
        await this.diagnosticsProvider.ProcessTextViewAsync(textViewSnapshot, cancellationToken);
    }
}
