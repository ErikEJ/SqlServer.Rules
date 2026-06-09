using Microsoft.VisualStudio.Shell.Interop;
using SqlAnalyzerSsms.Linter.Linting;

namespace SqlAnalyzerSsms.Linter.ErrorList
{
    /// <summary>
    /// Represents an error in the error list.
    /// </summary>
    internal sealed class SqlLintError
    {
        public string FilePath { get; }

        public int Line { get; }

        public int Column { get; }

        public string Message { get; }

        public string ErrorCode { get; }

        public string Description { get; }

        public string HelpLink { get; }

        public string ProjectName { get; }

        public __VSERRORCATEGORY Severity { get; }

        public SqlLintError(SqlAnalyzerDiagnosticInfo violation, string filePath, string projectName)
        {
            FilePath = filePath;
            Line = violation.Range.StartLine;
            Column = violation.Range.StartColumn;
            Message = violation.Message;
            ErrorCode = violation.ErrorCode;
            Description = violation.Message;
            HelpLink = violation.HelpLink?.ToString() ?? string.Empty;
            Severity = __VSERRORCATEGORY.EC_WARNING;
            ProjectName = projectName;
        }

        public SqlLintError(
            string filePath,
            int line,
            int column,
            string errorCode,
            string message,
            string description,
            string helpLink,
            string projectName,
            DiagnosticSeverity severity)
        {
            FilePath = filePath;
            Line = line;
            Column = column;
            ErrorCode = errorCode;
            Message = message;
            Description = description ?? string.Empty;
            HelpLink = helpLink ?? string.Empty;
            Severity = GetSeverity(severity);
            ProjectName = projectName;
        }

        private static __VSERRORCATEGORY GetSeverity(DiagnosticSeverity severity)
        {
            return severity switch
            {
                DiagnosticSeverity.Error => __VSERRORCATEGORY.EC_ERROR,
                DiagnosticSeverity.Warning => __VSERRORCATEGORY.EC_WARNING,
                _ => __VSERRORCATEGORY.EC_MESSAGE,
            };
        }
    }
}
