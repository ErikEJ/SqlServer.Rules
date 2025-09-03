using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;

namespace SqlAnalyzerExtension
{
    class AnalyzerIssuesSnapshot : WpfTableEntriesSnapshotBase
    {
        private readonly string filePath;
        private readonly int versionNumber;

        // We're not using an immutable list here but we cannot modify the list in any way once we've published the snapshot.
#pragma warning disable IDE1006 // Naming Styles
        public readonly List<Issue> Errors = new List<Issue>();
#pragma warning restore IDE1006 // Naming Styles


        internal AnalyzerIssuesSnapshot(string filePath, int versionNumber)
        {
            this.filePath = filePath;
            this.versionNumber = versionNumber;
        }

        public override int Count
        {
            get
            {
                return this.Errors.Count;
            }
        }

        public override int VersionNumber
        {
            get
            {
                return versionNumber;
            }
        }

        public override int IndexOf(int currentIndex, ITableEntriesSnapshot newerSnapshot)
        {
            return currentIndex;
        }

        public override bool TryGetValue(int index, string columnName, out object content)
        {
            if ((index >= 0) && (index < this.Errors.Count))
            {
                if (columnName == StandardTableKeyNames.DocumentName)
                {
                    // We return the full file path here. The UI handles displaying only the Path.GetFileName().
                    content = filePath;
                    return true;
                }
                else if (columnName == StandardTableKeyNames.ErrorCategory)
                {
                    content = "Documentation";
                    return true;
                }
                else if (columnName == StandardTableKeyNames.ErrorSource)
                {
                    content = "Spelling";
                    return true;
                }
                else if (columnName == StandardTableKeyNames.Line)
                {
                    // Line and column numbers are 0-based (the UI that displays the line/column number will add one to the value returned here).
                    content = this.Errors[index].SnapshotSpan.Start.GetContainingLine().LineNumber;

                    return true;
                }
                else if (columnName == StandardTableKeyNames.Column)
                {
                    var position = this.Errors[index].SnapshotSpan.Start;
                    var line = position.GetContainingLine();
                    content = position.Position - line.Start.Position;

                    return true;
                }
                else if (columnName == StandardTableKeyNames.Text)
                {
                    content = string.Format(CultureInfo.InvariantCulture, "Spelling: {0}", this.Errors[index].SnapshotSpan.GetText());

                    return true;
                }
                else if (columnName == StandardTableKeyNames2.TextInlines)
                {
                    var inlines = new List<Inline>();

                    inlines.Add(new Run("Spelling: "));
                    inlines.Add(new Run(this.Errors[index].SnapshotSpan.GetText())
                    {
                        FontWeight = FontWeights.ExtraBold
                    });

                    content = inlines;

                    return true;
                }
                else if (columnName == StandardTableKeyNames.ErrorSeverity)
                {
                    content = __VSERRORCATEGORY.EC_MESSAGE;

                    return true;
                }
                else if (columnName == StandardTableKeyNames.ErrorSource)
                {
                    content = ErrorSource.Other;

                    return true;
                }
                else if (columnName == StandardTableKeyNames.BuildTool)
                {
                    content = "SpellChecker";

                    return true;
                }
                else if (columnName == StandardTableKeyNames.ErrorCode)
                {
                    content = this.Errors[index].SnapshotSpan.GetText();

                    return true;
                }
                else if ((columnName == StandardTableKeyNames.ErrorCodeToolTip) || (columnName == StandardTableKeyNames.HelpLink))
                {
                    content = string.Format(CultureInfo.InvariantCulture, "http://www.bing.com/search?q={0}", this.Errors[index].SnapshotSpan.GetText());

                    return true;
                }

                // We should also be providing values for StandardTableKeyNames.Project & StandardTableKeyNames.ProjectName but that is
                // beyond the scope of this sample.
            }

            content = null;
            return false;
        }

    }
}
