using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace SqlAnalyzerExtension
{
    public class IssueTagger : ITagger<IErrorTag>
    {
        private readonly Analyzer analyzer;
        private List<Issue> errors = new List<Issue>();

        internal IEnumerable<Issue> Errors => errors;

        public IssueTagger(Analyzer analyzer)
        {
            this.analyzer = analyzer;
            _ = Task.Run(() => analyzer.AddTaggerAsync(this));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var error in errors)
            {
                if (spans.IntersectsWith(error.SnapshotSpan))
                {
                    yield return new TagSpan<IErrorTag>(
                        error.SnapshotSpan, new ErrorTag(PredefinedErrorTypeNames.Warning, error.Message.Message));
                }
            }
        }

        internal void UpdateErrors(ITextSnapshot snapshot, IEnumerable<Issue> errors)
        {
            if (errors == null)
            {
                ClearErrors(snapshot);
                return;
            }

            List<Issue> newErrors = new List<Issue>();
            int oldErrorsCount = 0;
            int newErrorsCount = 0;

            foreach (var error in errors)
            {
                if (this.errors.Contains(error))
                {
                    newErrors.Add(error);
                    oldErrorsCount++;
                }
                else
                {
                    newErrors.Add(error);
                    newErrorsCount++;
                }
            }

            if (oldErrorsCount != this.errors.Count || newErrorsCount > 0)
            {
                this.errors = newErrors;
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, this.errors.First().SnapshotSpan.Span)));
            }
        }

        internal void ClearErrors(ITextSnapshot currentSnapshot)
        {
            this.errors.Clear();
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(currentSnapshot, Microsoft.VisualStudio.Text.Span.FromBounds(0, 0))));
        }
    }
}
