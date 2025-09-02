using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace SqlAnalyzerExtension
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Analyzable)]
    public sealed class TaggerProvider : IViewTaggerProvider
    {
        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public string SourceTypeIdentifier => StandardTableDataSources.ErrorTableDataSource;

        public string Identifier => "TSQLAnalyzer";

        public string DisplayName => "T-SQL Analyzer";

        [ImportingConstructor]
        internal TaggerProvider()
        {
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer)
            where T : ITag
        {
            ITagger<T> tagger = null;

            if ((buffer == textView.TextBuffer) && (typeof(T) == typeof(IErrorTag)))
            {
                var analyzer = buffer.Properties.GetOrCreateSingletonProperty(typeof(Analyzer), () => new Analyzer(textView, buffer, TextDocumentFactoryService));
                if (analyzer.Tagger == null)
                {
                    tagger = new IssueTagger(analyzer) as ITagger<T>;
                }
                else
                {
                    tagger = analyzer.Tagger as ITagger<T>;
                }
            }

            return tagger;
        }
    }
}
