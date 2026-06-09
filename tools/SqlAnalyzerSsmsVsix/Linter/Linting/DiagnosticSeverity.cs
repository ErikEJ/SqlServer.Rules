namespace SqlAnalyzerSsms.Linter.Linting
{
    /// <summary>
    /// Represents a diagnostic severity level for sql lint rules.
    /// Maps to EditorConfig severity values and Visual Studio error types.
    /// </summary>
    public enum DiagnosticSeverity
    {
        /// <summary>Rule is disabled</summary>
        None,

        /// <summary>Warning - shown as warning squiggle</summary>
        Warning,

        /// <summary>Error - shown as error squiggle</summary>
        Error,
    }
}
