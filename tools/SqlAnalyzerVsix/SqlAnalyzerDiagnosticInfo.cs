using System;
using Microsoft.VisualStudio.Extensibility.Languages;
using Range = Microsoft.VisualStudio.RpcContracts.Utilities.Range;

namespace SqlAnalyzer;

/// <summary>
/// Class that contains diagnostic information found by the SQL analyzer.
/// Holds information to be converted to a <see cref="DocumentDiagnostic"/>.
/// </summary>
public class SqlAnalyzerDiagnosticInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlAnalyzerDiagnosticInfo"/> class.
    /// </summary>
    /// <param name="range">Range where the diagnostic exists.</param>
    /// <param name="message">Message to be presented with the diagnostic.</param>
    /// <param name="errorCode">Unique error code of this type of diagnostic.</param>
    /// <param name="helpLink">Help URL</param>
    public SqlAnalyzerDiagnosticInfo(Range range, string message, string errorCode, Uri? helpLink)
    {
        this.Range = range;
        this.Message = message;
        this.ErrorCode = errorCode;
        this.HelpLink = helpLink;
    }

    public Uri? HelpLink { get; set; }

    /// <summary>
    /// Gets the range of the diagnostic.
    /// </summary>
    public Range Range { get; }

    /// <summary>
    /// Gets the error message of the diagnostic.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the error code of the diagnostic.
    /// </summary>
    public string ErrorCode { get; }
}
