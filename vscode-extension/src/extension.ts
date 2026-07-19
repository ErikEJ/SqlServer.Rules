import * as vscode from 'vscode';
import { AnalyzerClient, ServerProblem, ServerResponse } from './client';

let client: AnalyzerClient | undefined;
let diagnostics: vscode.DiagnosticCollection;
let output: vscode.OutputChannel;
let statusBar: vscode.StatusBarItem | undefined;

const debounceTimers = new Map<string, NodeJS.Timeout>();
/** Latest analysis token per document, used to discard superseded (stale) responses. */
const latestToken = new Map<string, number>();
/** Most recent problems per document, keyed by document URI, used to answer hover requests. */
const documentProblems = new Map<string, ServerProblem[]>();
let tokenCounter = 0;
/** Number of analysis requests currently in flight, used to drive the status bar. */
let inFlightCount = 0;
/** Whether an analyzer-server failure has already been surfaced to the user (reset on success). */
let serverFailureReported = false;

export function activate(context: vscode.ExtensionContext): void {
    output = vscode.window.createOutputChannel('T-SQL Analyzer');
    diagnostics = vscode.languages.createDiagnosticCollection('tsqlAnalyzer');
    statusBar = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 100);
    statusBar.command = 'tsqlAnalyzer.analyzeActiveFile';

    context.subscriptions.push(output, diagnostics, statusBar);
    updateStatusBar();

    context.subscriptions.push(
        vscode.languages.registerHoverProvider('sql', { provideHover }),
        vscode.workspace.onDidOpenTextDocument((doc) => scheduleAnalysis(doc, 0)),
        vscode.workspace.onDidChangeTextDocument((e) => scheduleAnalysis(e.document)),
        vscode.workspace.onDidSaveTextDocument((doc) => scheduleAnalysis(doc, 0)),
        vscode.workspace.onDidCloseTextDocument((doc) => clearDocument(doc)),
        vscode.workspace.onDidChangeConfiguration((e) => {
            if (e.affectsConfiguration('tsqlAnalyzer')) {
                updateStatusBar();
                restartClient();
                analyzeAllOpenDocuments();
            }
        }),
        vscode.commands.registerCommand('tsqlAnalyzer.restartServer', () => {
            restartClient();
            analyzeAllOpenDocuments();
        }),
        vscode.commands.registerCommand('tsqlAnalyzer.analyzeActiveFile', () => {
            const doc = vscode.window.activeTextEditor?.document;
            if (doc) {
                scheduleAnalysis(doc, 0);
            }
        }),
    );

    // Analyze documents that are already open at activation time.
    analyzeAllOpenDocuments();
}

export function deactivate(): void {
    for (const timer of debounceTimers.values()) {
        clearTimeout(timer);
    }
    debounceTimers.clear();
    documentProblems.clear();
    client?.dispose();
    client = undefined;
}

function isSqlDocument(doc: vscode.TextDocument): boolean {
    return doc.languageId === 'sql';
}

function analyzeAllOpenDocuments(): void {
    for (const doc of vscode.workspace.textDocuments) {
        scheduleAnalysis(doc, 0);
    }
}

function scheduleAnalysis(doc: vscode.TextDocument, delayOverrideMs?: number): void {
    if (!isSqlDocument(doc)) {
        return;
    }

    const config = vscode.workspace.getConfiguration('tsqlAnalyzer');
    if (!config.get<boolean>('enable', true)) {
        return;
    }

    const key = doc.uri.toString();
    const existing = debounceTimers.get(key);
    if (existing) {
        clearTimeout(existing);
    }

    const delay = delayOverrideMs ?? config.get<number>('debounceMs', 400);
    const timer = setTimeout(() => {
        debounceTimers.delete(key);
        void analyze(doc);
    }, Math.max(0, delay));

    debounceTimers.set(key, timer);
}

async function analyze(doc: vscode.TextDocument): Promise<void> {
    const config = vscode.workspace.getConfiguration('tsqlAnalyzer');
    const key = doc.uri.toString();
    const token = ++tokenCounter;
    latestToken.set(key, token);

    inFlightCount++;
    updateStatusBar();

    let response: ServerResponse;
    try {
        response = await getClient().analyze({
            // Send the on-disk path alongside the in-memory content so older analyzer
            // servers (which only understand `path`) keep working; newer servers prefer
            // `content`, giving accurate results for unsaved edits.
            path: doc.uri.scheme === 'file' ? doc.uri.fsPath : undefined,
            content: doc.getText(),
            rules: config.get<string>('rules', '') || undefined,
            sqlVersion: config.get<string>('sqlVersion') || undefined,
            additionalAnalyzers: config.get<string[]>('additionalAnalyzers', []),
        });
    } catch (err) {
        const message = err instanceof Error ? err.message : String(err);
        output.appendLine(`Analysis failed for ${doc.uri.fsPath}: ${message}`);
        reportServerFailure(message);
        diagnostics.delete(doc.uri);
        documentProblems.delete(key);
        return;
    } finally {
        inFlightCount = Math.max(0, inFlightCount - 1);
        updateStatusBar();
    }

    // Discard responses that have been superseded by a newer edit.
    if (latestToken.get(key) !== token) {
        return;
    }

    if (response.status === 'error') {
        const message = response.error ?? 'unknown error';
        output.appendLine(`Analyzer error for ${doc.uri.fsPath}: ${message}`);
        reportServerFailure(message);
        diagnostics.delete(doc.uri);
        documentProblems.delete(key);
        return;
    }

    // A response was received, so the server is healthy again; allow future failures
    // to be surfaced to the user.
    serverFailureReported = false;
    const problems = response.problems ?? [];
    documentProblems.set(key, problems);
    diagnostics.set(doc.uri, problems.map((p) => toDiagnostic(p)));
}

/**
 * Surfaces an analyzer-server startup/communication failure to the user once, so a
 * missing .NET SDK or unreachable analyzer does not result in a silent lack of diagnostics.
 * Subsequent failures are only written to the output channel until analysis succeeds again.
 */
function reportServerFailure(message: string): void {
    if (serverFailureReported) {
        return;
    }
    serverFailureReported = true;

    void vscode.window
        .showErrorMessage(
            `T-SQL Analyzer could not run the analysis server: ${message}. Ensure the .NET 10 SDK is installed, or set 'tsqlAnalyzer.serverPath'.`,
            'Show Log',
        )
        .then((choice) => {
            if (choice === 'Show Log') {
                output.show(true);
            }
        });
}

/**
 * Reflects the current analysis state in the status bar: a spinning "Analyzing…"
 * indicator while requests are in flight, otherwise an idle T-SQL Analyzer label.
 */
function updateStatusBar(): void {
    if (!statusBar) {
        return;
    }

    const enabled = vscode.workspace.getConfiguration('tsqlAnalyzer').get<boolean>('enable', true);
    if (!enabled) {
        statusBar.hide();
        return;
    }

    if (inFlightCount > 0) {
        statusBar.text = '$(sync~spin) Analyzing…';
        statusBar.tooltip = 'T-SQL Analyzer is analyzing the current SQL';
    } else {
        statusBar.text = '$(check) T-SQL Analyzer';
        statusBar.tooltip = 'T-SQL Analyzer — click to analyze the active file';
    }

    statusBar.show();
}

function toDiagnostic(problem: ServerProblem): vscode.Diagnostic {
    const range = toRange(problem);
    const diagnostic = new vscode.Diagnostic(range, problem.message, toSeverity(problem.severity));
    diagnostic.source = 'T-SQL Analyzer';

    if (problem.helpLink) {
        diagnostic.code = {
            value: problem.rule,
            target: vscode.Uri.parse(problem.helpLink),
        };
    } else {
        diagnostic.code = problem.rule;
    }

    return diagnostic;
}

function toRange(problem: ServerProblem): vscode.Range {
    // Server positions are 1-based; VS Code ranges are 0-based.
    const startLine = Math.max(0, problem.line - 1);
    const startCol = Math.max(0, problem.column - 1);
    const endLine = Math.max(startLine, problem.endLine - 1);
    const endCol = Math.max(0, problem.endColumn - 1);

    return new vscode.Range(startLine, startCol, endLine, endCol);
}

/**
 * Provides a hover for T-SQL Analyzer diagnostics under the cursor, showing each rule id,
 * its description (the analyzer message) and a link to the generated docs/**\/SR*.md page.
 */
function provideHover(
    document: vscode.TextDocument,
    position: vscode.Position,
): vscode.Hover | undefined {
    const problems = documentProblems.get(document.uri.toString());
    if (!problems || problems.length === 0) {
        return undefined;
    }

    const matches = problems.filter((p) => toRange(p).contains(position));
    if (matches.length === 0) {
        return undefined;
    }

    const markdown = new vscode.MarkdownString();
    matches.forEach((problem, index) => {
        if (index > 0) {
            markdown.appendMarkdown('\n\n---\n\n');
        }

        markdown.appendMarkdown(`**T-SQL Analyzer — ${escapeMarkdown(problem.rule)}**\n\n`);
        markdown.appendMarkdown(`${escapeMarkdown(problem.message)}`);

        if (problem.helpLink) {
            markdown.appendMarkdown(`\n\n[View rule documentation](${problem.helpLink})`);
        }
    });

    return new vscode.Hover(markdown);
}

/** Escapes Markdown control characters so rule text is rendered verbatim in a hover. */
function escapeMarkdown(text: string): string {
    return text.replace(/([\\`*_{}\[\]()#+\-.!|<>])/g, '\\$1');
}

function toSeverity(severity: string): vscode.DiagnosticSeverity {
    switch ((severity || '').toLowerCase()) {
        case 'error':
            return vscode.DiagnosticSeverity.Error;
        case 'information':
        case 'info':
            return vscode.DiagnosticSeverity.Information;
        case 'hint':
            return vscode.DiagnosticSeverity.Hint;
        default:
            return vscode.DiagnosticSeverity.Warning;
    }
}

function clearDocument(doc: vscode.TextDocument): void {
    const key = doc.uri.toString();
    const timer = debounceTimers.get(key);
    if (timer) {
        clearTimeout(timer);
        debounceTimers.delete(key);
    }
    latestToken.delete(key);
    documentProblems.delete(key);
    diagnostics.delete(doc.uri);
}

function getClient(): AnalyzerClient {
    if (!client) {
        client = createClient();
    }
    return client;
}

function restartClient(): void {
    client?.dispose();
    client = createClient();
    diagnostics.clear();
    latestToken.clear();
    documentProblems.clear();
    serverFailureReported = false;
}

function createClient(): AnalyzerClient {
    const { command, args } = resolveServerCommand();
    output.appendLine(`Starting analyzer server: ${command} ${args.join(' ')}`);
    return new AnalyzerClient({
        command,
        args,
        log: (message) => output.appendLine(message),
    });
}

function resolveServerCommand(): { command: string; args: string[] } {
    const config = vscode.workspace.getConfiguration('tsqlAnalyzer');
    const configuredPath = (config.get<string>('serverPath', '') || '').trim();
    if (configuredPath) {
        return { command: configuredPath, args: ['--server-mode'] };
    }

    // Run the analyzer CLI as a NuGet package via the .NET 10 SDK `dnx` command,
    // mirroring how the Visual Studio and SSMS extensions launch it. This requires
    // the .NET 10 SDK to be installed.
    //
    // We invoke `dotnet dnx` rather than the bare `dnx` launcher: the SDK reliably
    // puts `dotnet` on the PATH, whereas the standalone `dnx` shim is often not on
    // the PATH (leading to a silent `spawn dnx ENOENT` and no diagnostics).
    return {
        command: 'dotnet',
        args: ['dnx', 'ErikEJ.DacFX.TSQLAnalyzer.Cli', '--yes', '--', '--server-mode'],
    };
}
