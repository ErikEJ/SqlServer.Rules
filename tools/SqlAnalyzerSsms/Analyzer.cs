using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;

namespace SqlAnalyzerExtension
{
    public class Analyzer
    {
        private readonly ITextView textView;
        private readonly ITextBuffer buffer;
        private readonly bool canBeAnalyzed = true;
        private readonly string filePath;
        private bool isAnalyzed = false;
        private string tempPath = Path.Combine(Path.GetTempPath(), "tsqlanalyzerscratch.sql");

#pragma warning disable IDE1006 // Naming Styles
        internal readonly AnalyzerIssuesFactory Factory;
#pragma warning restore IDE1006 // Naming Styles

        private static readonly Regex SqlAnalyzerOutputRegex = new Regex(
            @"^(?<filename>.+?)\((?<line>\d+),(?<column>\d+)\):\s*(?<ruleid>[^:]+)\s*:\s*(?<description>.*)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5));

        public IssueTagger Tagger { get; private set; } = null;

        public Analyzer(ITextView textView, ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactoryService)
        {
            this.textView = textView;
            this.buffer = buffer;
            if (textDocumentFactoryService.TryGetTextDocument(buffer, out var document))
            {
                this.filePath = document.FilePath;
                System.IO.FileInfo fi = new System.IO.FileInfo(document.FilePath);
                canBeAnalyzed = fi.Extension.Equals(".sql", StringComparison.OrdinalIgnoreCase);
            }

            this.Factory = new AnalyzerIssuesFactory(this, new AnalyzerIssuesSnapshot(this.filePath, 0));
        }

        internal async Task AddTaggerAsync(IssueTagger tagger)
        {
            if (this.Tagger == null)
            {
                Tagger = tagger;
                buffer.ChangedLowPriority += Buffer_Changed;
                await AnalyzeAsync();
            }
        }

        private async void Buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            await AnalyzeAsync();
        }

        private async Task AnalyzeAsync()
        {
            if (!isAnalyzed && canBeAnalyzed)
            {
                isAnalyzed = true;
                string text = textView.TextBuffer.CurrentSnapshot.GetText();

                IEnumerable<DiagnosticMessage> analyzeResult = await RunAnalyzerOnDocumentAsync(buffer.CurrentSnapshot, null, null, CancellationToken.None);

                if (analyzeResult.Any())
                {
                    List<Issue> errors = new List<Issue>();
                    foreach (var message in analyzeResult)
                    {
                        errors.Add(
                            new Issue(
                                new SnapshotSpan(buffer.CurrentSnapshot, message.Span.From, message.Span.Length), message));
                    }

                    Tagger.UpdateErrors(buffer.CurrentSnapshot, errors);
                }
                else
                {
                    Tagger.ClearErrors(buffer.CurrentSnapshot);
                }

                isAnalyzed = false;
            }
        }

        public async Task<IEnumerable<DiagnosticMessage>> RunAnalyzerOnDocumentAsync(ITextSnapshot textDocument, string rules, string sqlVersion, CancellationToken cancellationToken)
        {
            using (var analyzer = new Process())
            {
                var lineQueue = new AsyncQueue<string>();

                if (textDocument.Length > 8192)
                {
                    // SQL analyzer has issues processing very large files.
                    return Array.Empty<DiagnosticMessage>();
                }

                var content = textDocument.GetText();

                File.WriteAllText(tempPath, content, Encoding.UTF8);

                StartAnalyzerProcess(analyzer, lineQueue, tempPath, rules, sqlVersion);

                return await ProcessAnalyzerQueueAsync(lineQueue);
            }
        }

        private static void StartAnalyzerProcess(Process analyzer, AsyncQueue<string> lineQueue, string path, string rules, string sqlVersion)
        {
            string args = "/c \"tsqlanalyze -n -i" +
                $" \"{path}\"\"";

            if (!string.IsNullOrWhiteSpace(rules))
            {
                args = args + $" -r Rules:{rules}";
            }

            if (!string.IsNullOrWhiteSpace(sqlVersion))
            {
                args = args + $" -s {sqlVersion}";
            }

            analyzer.StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            analyzer.EnableRaisingEvents = true;
            analyzer.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (e.Data != null)
                {
                    lineQueue.Enqueue(e.Data);
                }
                else
                {
                    lineQueue.Complete();
                }
            });

            try
            {
                analyzer.Start();
                analyzer.BeginOutputReadLine();
            }
            catch (Win32Exception ex)
            {
                throw new InvalidOperationException(message: ex.Message, innerException: ex);
            }
        }

        private static async Task<IEnumerable<DiagnosticMessage>> ProcessAnalyzerQueueAsync(AsyncQueue<string> lineQueue)
    {
            Requires.NotNull(lineQueue, nameof(lineQueue));

            var diagnostics = new List<DiagnosticMessage>();

            while (!(lineQueue.IsCompleted && lineQueue.IsEmpty))
            {
                string line;
                try
                {
                    line = await lineQueue.DequeueAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                var diagnostic = line != null ? GetDiagnosticFromAnalyzerOutput(line) : null;
                if (diagnostic != null)
                {
                    diagnostics.Add(diagnostic);
                }
                else
                {
                    // Something went wrong so break and return the current set.
                    break;
                }
            }

            return diagnostics;
        }

        private static DiagnosticMessage GetDiagnosticFromAnalyzerOutput(string outputLine)
        {
            Requires.NotNull(outputLine, nameof(outputLine));

            var parsed = ParseSqlAnalyzerOutput(outputLine);

            if (parsed is null)
            {
                // The output line does not match the expected format.
                return null;
            }

            var ruleId = parsed.Value.RuleId.Split('.').Last();
            var rulePrefix = parsed.Value.RuleId.Replace("." + ruleId, string.Empty);
            var description = rulePrefix + ": " + parsed.Value.Description;
            var span = new Span(parsed.Value.Column - 1, parsed.Value.Line - 1);

            return new DiagnosticMessage(span, description, Severity.Warning);
        }

        /// <summary>
        /// Parses a SQL analyzer output line and extracts its components.
        /// </summary>
        /// <param name="outputLine">The output line to parse</param>
        /// <returns>A tuple containing the parsed components, or null if parsing fails</returns>
        private static (string Filename, int Line, int Column, string RuleId, string Description)? ParseSqlAnalyzerOutput(string outputLine)
        {
            if (string.IsNullOrWhiteSpace(outputLine))
            {
                return null;
            }

            var match = SqlAnalyzerOutputRegex.Match(outputLine);
            if (!match.Success)
            {
                return null;
            }

            if (!int.TryParse(match.Groups["line"].Value, out int line) ||
                !int.TryParse(match.Groups["column"].Value, out int column))
            {
                return null;
            }

            return (
                Filename: match.Groups["filename"].Value,
                Line: line,
                Column: column,
                RuleId: match.Groups["ruleid"].Value.Trim(),
                Description: match.Groups["description"].Value.Trim()
            );
        }
    }
}
