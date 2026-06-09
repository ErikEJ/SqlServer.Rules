using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;

namespace SqlAnalyzerSsms.Linter.Linting
{
    /// <summary>
    /// Provides shared analysis caching for T-SQL documents. Both the tagger and error list use this to avoid
    /// duplicate parsing.
    /// </summary>
    [Export(typeof(SqlAnalysisCache))]
    public class SqlAnalysisCache
    {
        /// <summary>
        /// Delay in milliseconds before analyzing after the last keystroke.
        /// </summary>
        private const int _debounceDelayMs = 300;

        private static readonly object _propertyKey = typeof(SqlAnalysisCache);
        private static readonly object _pendingAnalysisKey = typeof(SqlAnalysisCache).FullName + ".PendingAnalysis";

        /// <summary>
        /// Event raised when analysis results are updated for a buffer.
        /// </summary>
        public event EventHandler<AnalysisUpdatedEventArgs> AnalysisUpdated;

        public void AnalyzeImmediate(ITextBuffer buffer, string filePath, string sqlVersion, string rules, string projectName)
        {
            ITextSnapshot snapshot = buffer.CurrentSnapshot;
            if (HasPendingAnalysisForSnapshot(buffer, snapshot.Version.VersionNumber))
            {
                return;
            }

            // Cancel any pending debounced analysis
            CancelPendingAnalysis(buffer);

            var text = snapshot.GetText();

            // Run analysis on a background thread without debounce delay
#pragma warning disable CA2000 // Dispose objects before losing scope
            var cts = new CancellationTokenSource();
#pragma warning restore CA2000 // Dispose objects before losing scope
            var pendingAnalysis = new PendingAnalysis(cts, snapshot.Version.VersionNumber);
            buffer.Properties[_pendingAnalysisKey] = pendingAnalysis;
            PerformAnalysisNowAsync(buffer, filePath, sqlVersion, rules, projectName, snapshot, text, pendingAnalysis.CancellationTokenSource.Token).FireAndForget();
        }

        /// <summary>
        /// Triggers debounced analysis on a background thread. Waits for a pause in typing before analyzing to reduce
        /// CPU usage. Use this when the buffer content changes during editing.
        /// </summary>
        public void InvalidateAndAnalyze(ITextBuffer buffer, string filePath, string sqlVersion, string rules, string projectName)
        {
            // Cancel any pending analysis for this buffer
            CancelPendingAnalysis(buffer);

#pragma warning disable CA2000 // Dispose objects before losing scope
            var cts = new CancellationTokenSource();
#pragma warning restore CA2000 // Dispose objects before losing scope
            ITextSnapshot snapshot = buffer.CurrentSnapshot;
            buffer.Properties[_pendingAnalysisKey] = new PendingAnalysis(cts, snapshot.Version.VersionNumber);
            var text = snapshot.GetText();

            // Pass the token, not the CTS, to avoid accessing disposed CTS
            PerformAnalysisAsync(buffer, filePath, sqlVersion, rules, projectName, snapshot, text, cts.Token).FireAndForget();
        }

        private async Task PerformAnalysisAsync(ITextBuffer buffer, string filePath, string sqlVersion, string rules, string projectName, ITextSnapshot snapshot, string text, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(_debounceDelayMs, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    PerformAnalysis(buffer, snapshot, text, filePath, sqlVersion, rules, projectName, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when user types again before delay expires
            }
            catch (ObjectDisposedException)
            {
                // CancellationTokenSource was disposed - this is fine, just stop
            }
        }

        private async Task PerformAnalysisNowAsync(ITextBuffer buffer, string filePath, string sqlVersion, string rules, string projectName, ITextSnapshot snapshot, string text, CancellationToken cancellationToken)
        {
            try
            {
                // Yield to background thread immediately (no debounce delay)
                await Task.Run(() => PerformAnalysis(buffer, snapshot, text, filePath, sqlVersion, rules, projectName, cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Analysis was cancelled
            }
            catch (ObjectDisposedException)
            {
                // CancellationTokenSource was disposed
            }
            finally
            {
                ClearPendingAnalysisIfSnapshotMatches(buffer, snapshot.Version.VersionNumber);
            }
        }

        /// <summary>
        /// Performs the actual analysis and updates the cache.
        /// </summary>
        private void PerformAnalysis(ITextBuffer buffer, ITextSnapshot snapshot, string text, string filePath, string sqlVersion, string rules, string projectName, CancellationToken cancellationToken = default)
        {
            try
            {
                // Return empty violations if linting is disabled
                var violations = new List<SqlAnalyzerDiagnosticInfo>();

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    violations = await AnalyzerUtilities.Instance.AnalyzeAsync(text, rules, sqlVersion, cancellationToken);
                });

                var result = new CachedAnalysisResult(snapshot.Version.VersionNumber, violations);

                buffer.Properties[_propertyKey] = result;

                AnalysisUpdated?.Invoke(this, new AnalysisUpdatedEventArgs(buffer, snapshot, violations, filePath, projectName));
            }
            catch (OperationCanceledException)
            {
                // Analysis was cancelled — don't update cache or notify listeners
            }
            catch (Exception ex)
            {
                ex.Log("Script analysis failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Cancels any pending debounced analysis for the buffer.
        /// </summary>
        private void CancelPendingAnalysis(ITextBuffer buffer)
        {
            if (buffer.Properties.TryGetProperty(_pendingAnalysisKey, out PendingAnalysis pendingAnalysis))
            {
                _ = buffer.Properties.RemoveProperty(_pendingAnalysisKey);

                // Cancel first, then dispose - order matters for race condition safety
                // The token is passed by value to the async method, so accessing IsCancellationRequested
                // after Cancel() is safe, but we should not dispose until after Task.Delay returns
                try
                {
                    pendingAnalysis.CancellationTokenSource.Cancel();
                }
                finally
                {
                    // Dispose is safe here because Task.Delay will throw OperationCanceledException
                    // before accessing the CTS again, and we catch ObjectDisposedException as a fallback
                    pendingAnalysis.CancellationTokenSource.Dispose();
                }
            }
        }

        private static bool HasPendingAnalysisForSnapshot(ITextBuffer buffer, int snapshotVersion)
        {
            return buffer.Properties.TryGetProperty(_pendingAnalysisKey, out PendingAnalysis pendingAnalysis)
                && pendingAnalysis.SnapshotVersion == snapshotVersion;
        }

        private static void ClearPendingAnalysisIfSnapshotMatches(ITextBuffer buffer, int snapshotVersion)
        {
            if (buffer.Properties.TryGetProperty(_pendingAnalysisKey, out PendingAnalysis pendingAnalysis)
                && pendingAnalysis.SnapshotVersion == snapshotVersion)
            {
                _ = buffer.Properties.RemoveProperty(_pendingAnalysisKey);
                pendingAnalysis.CancellationTokenSource.Dispose();
            }
        }
    }
}
