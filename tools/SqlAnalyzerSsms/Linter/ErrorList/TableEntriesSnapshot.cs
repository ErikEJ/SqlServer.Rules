using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.TableManager;

namespace SqlAnalyzerSsms.Linter.ErrorList
{
    /// <summary>
    /// Snapshot of error entries for a file.
    /// </summary>
    internal sealed class TableEntriesSnapshot(string filePath, List<SqlLintError> errors) : ITableEntriesSnapshot
    {
        public string FilePath { get; } = filePath;

        public int VersionNumber { get; } = 1;

        public int Count => errors.Count;

        public IEnumerable<SqlLintError> GetErrors() => errors;

        public int IndexOf(int currentIndex, ITableEntriesSnapshot newSnapshot)
        {
            return currentIndex;
        }

        public bool TryGetValue(int index, string keyName, out object? content)
        {
            if (index < 0 || index >= errors.Count)
            {
                content = null;
                return false;
            }

            SqlLintError error = errors[index];

            switch (keyName)
            {
                case StandardTableKeyNames.DocumentName:
                    content = error.DocumentName;
                    return true;

                case StandardTableKeyNames.Path:
                    content = error.FilePath;
                    return true;

                case StandardTableKeyNames.Line:
                    content = error.Line;
                    return true;

                case StandardTableKeyNames.Column:
                    content = error.Column;
                    return true;

                case StandardTableKeyNames.Text:
                    content = error.Message;
                    return true;

                case StandardTableKeyNames.ErrorCode:
                    content = error.ErrorCode;
                    return true;

                case StandardTableKeyNames.ErrorSeverity:
                    content = error.Severity;
                    return true;

                case StandardTableKeyNames.ErrorCategory:
                    content = "SQL";
                    return true;

                case StandardTableKeyNames.BuildTool:
                    content = "TSqlAnalyzer";
                    return true;

                case StandardTableKeyNames.HelpLink:
                    content = error.HelpLink;
                    return true;

                case StandardTableKeyNames.ErrorCodeToolTip:
                    content = error.Description;
                    return true;

                case StandardTableKeyNames.ProjectName:
                    content = error.ProjectName;
                    return true;

                default:
                    content = null;
                    return false;
            }
        }

        public void StartCaching()
        {
        }

        public void StopCaching()
        {
        }

        public void Dispose()
        {
        }
    }
}
