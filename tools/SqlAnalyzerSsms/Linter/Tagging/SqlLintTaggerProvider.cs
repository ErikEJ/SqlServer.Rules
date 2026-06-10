using System;
using System.ComponentModel.Composition;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using SqlAnalyzerSsms.Extensions;
using SqlAnalyzerSsms.Linter.Linting;
using SqlAnalyzerSsms.Options;

namespace SqlAnalyzerSsms.Linter.Tagging
{
    /// <summary>
    /// Provides the tagger for sql files.
    /// </summary>
    [Export(typeof(ITaggerProvider))]
    [ContentType("SQL")]
    [ContentType("SQL Server Tools")]
    [TagType(typeof(IErrorTag))]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public class SqlLintTaggerProvider : ITaggerProvider
    {
        [Import]
        internal SqlAnalysisCache AnalysisCache { get; set; }

        public ITagger<T>? CreateTagger<T>(ITextBuffer buffer)
            where T : ITag
        {
            if (buffer == null)
            {
                return null;
            }

            if (!buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document)
                || string.IsNullOrEmpty(document.FilePath)
                || !document.FilePath.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var enabled = true;

            // TODO Update when new version is available
            var sqlVersion = "Sql170";

            // TODO Enable user to set this in Options
            var rules = string.Empty;
            var projectName = string.Empty;

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var options = await ToolOptions.GetLiveInstanceAsync();
                enabled = !options.DisableCodeAnalysis;
                if (!enabled)
                {
                    return;
                }
                var currentProject = await VS.Solutions.GetActiveProjectAsync();

                if (currentProject != null)
                {
                    projectName = currentProject.Name;
                    var runProperties = await currentProject.IsInSqlProjAsync();
                    enabled = runProperties.Run;
                    sqlVersion = runProperties.SqlVersion;
                    rules = runProperties.Rules;
                }
            });

            if (!enabled)
            {
                return null;
            }

            return buffer.Properties.GetOrCreateSingletonProperty(
                typeof(SqlLintTagger),
                () => new SqlLintTagger(buffer, AnalysisCache, sqlVersion, rules, projectName)) as ITagger<T>;
        }
    }
}
