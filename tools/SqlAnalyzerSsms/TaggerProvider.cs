using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace SqlAnalyzerExtension
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Analyzable)]
    public sealed class TaggerProvider : IViewTaggerProvider, ITableDataSource
    {
        private readonly List<SinkManager> managers = new List<SinkManager>();      // Also used for locks
        private readonly List<Analyzer> analyzers = new List<Analyzer>();
        private readonly ITableManager errorTableManager;
        private readonly ITextDocumentFactoryService textDocumentFactoryService;


        public string SourceTypeIdentifier => StandardTableDataSources.ErrorTableDataSource;

        public string Identifier => "TSQLAnalyzer";

        public string DisplayName => "T-SQL Analyzer";

        [ImportingConstructor]
        internal TaggerProvider([Import]ITableManagerProvider provider, [Import] ITextDocumentFactoryService textDocumentFactoryService)
        {
            this.errorTableManager = provider.GetTableManager(StandardTables.ErrorsTable);
            this.textDocumentFactoryService = textDocumentFactoryService;

            this.errorTableManager.AddSource(this, StandardTableColumnDefinitions.DetailsExpander, 
                                                   StandardTableColumnDefinitions.ErrorSeverity, StandardTableColumnDefinitions.ErrorCode,
                                                   StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.BuildTool,
                                                   StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.ErrorCategory,
                                                   StandardTableColumnDefinitions.Text, StandardTableColumnDefinitions.DocumentName, StandardTableColumnDefinitions.Line, StandardTableColumnDefinitions.Column);
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer)
            where T : ITag
        {
            ITagger<T> tagger = null;

            if ((buffer == textView.TextBuffer) && (typeof(T) == typeof(IErrorTag)))
            {
                var analyzer = buffer.Properties.GetOrCreateSingletonProperty(typeof(Analyzer), () => new Analyzer(textView, buffer, textDocumentFactoryService));
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

        public IDisposable Subscribe(ITableDataSink sink)
        {
            // This method is called to each consumer interested in errors. In general, there will be only a single consumer (the error list tool window)
            // but it is always possible for 3rd parties to write code that will want to subscribe.
            return new SinkManager(this, sink);
        }

        public void AddSinkManager(SinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (managers)
            {
                managers.Add(manager);

                // Add the pre-existing spell checkers to the manager.
                foreach (var analyzer in analyzers)
                {
                    manager.AddAnalyzer(analyzer);
                }
            }
        }

        public void RemoveSinkManager(SinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (managers)
            {
                managers.Remove(manager);
            }
        }

        public void AddAnalyzer(Analyzer analyzer)
        {
            // This call will always happen on the UI thread (it is a side-effect of adding or removing the 1st/last tagger).
            lock (managers)
            {
                analyzers.Add(analyzer);

                // Tell the preexisting managers about the new analyzer
                foreach (var manager in managers)
                {
                    manager.AddAnalyzer(analyzer);
                }
            }
        }

        public void RemoveSpellChecker(Analyzer analyzer)
        {
            // This call will always happen on the UI thread (it is a side-effect of adding or removing the 1st/last tagger).
            lock (managers)
            {
                analyzers.Remove(analyzer);

                foreach (var manager in managers)
                {
                    manager.RemoveAnalyzer(analyzer);
                }
            }
        }

    }
}
