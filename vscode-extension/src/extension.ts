import * as vscode from 'vscode';
import { AnalyzerClient, ServerProblem, ServerResponse } from './client';

let client: AnalyzerClient | undefined;
let diagnostics: vscode.DiagnosticCollection;
let output: vscode.OutputChannel;

const debounceTimers = new Map<string, NodeJS.Timeout>();
/** Latest analysis token per document, used to discard superseded (stale) responses. */
const latestToken = new Map<string, number>();
let tokenCounter = 0;

export function activate(context: vscode.ExtensionContext): void {
    output = vscode.window.createOutputChannel('T-SQL Analyzer');
    diagnostics = vscode.languages.createDiagnosticCollection('tsqlAnalyzer');

    context.subscriptions.push(output, diagnostics);

    context.subscriptions.push(
        vscode.workspace.onDidOpenTextDocument((doc) => scheduleAnalysis(doc, 0)),
        vscode.workspace.onDidChangeTextDocument((e) => scheduleAnalysis(e.document)),
        vscode.workspace.onDidSaveTextDocument((doc) => scheduleAnalysis(doc, 0)),
        vscode.workspace.onDidCloseTextDocument((doc) => clearDocument(doc)),
        vscode.workspace.onDidChangeConfiguration((e) => {
            if (e.affectsConfiguration('tsqlAnalyzer')) {
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

    let response: ServerResponse;
    try {
        response = await getClient().analyze({
            content: doc.getText(),
            rules: config.get<string>('rules', '') || undefined,
            sqlVersion: config.get<string>('sqlVersion', 'Sql160'),
            additionalAnalyzers: config.get<string[]>('additionalAnalyzers', []),
        });
    } catch (err) {
        output.appendLine(`Analysis failed for ${doc.uri.fsPath}: ${err instanceof Error ? err.message : String(err)}`);
        return;
    }

    // Discard responses that have been superseded by a newer edit.
    if (latestToken.get(key) !== token) {
        return;
    }

    if (response.status === 'error') {
        output.appendLine(`Analyzer error for ${doc.uri.fsPath}: ${response.error ?? 'unknown error'}`);
        return;
    }

    diagnostics.set(doc.uri, (response.problems ?? []).map((p) => toDiagnostic(p)));
}

function toDiagnostic(problem: ServerProblem): vscode.Diagnostic {
    // Server positions are 1-based; VS Code ranges are 0-based.
    const startLine = Math.max(0, problem.line - 1);
    const startCol = Math.max(0, problem.column - 1);
    const endLine = Math.max(startLine, problem.endLine - 1);
    const endCol = Math.max(0, problem.endColumn - 1);

    const range = new vscode.Range(startLine, startCol, endLine, endCol);
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
    return {
        command: 'dnx',
        args: ['ErikEJ.DacFX.TSQLAnalyzer.Cli', '--yes', '--', '--server-mode'],
    };
}
