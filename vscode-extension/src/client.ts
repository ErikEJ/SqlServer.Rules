import * as cp from 'child_process';
import * as readline from 'readline';

/**
 * A single analysis problem returned by the server (mirrors ServerProblem in
 * ErikEJ.DacFX.TSQLAnalyzer.Protocol). Line/column values are 1-based.
 */
export interface ServerProblem {
    rule: string;
    line: number;
    column: number;
    endLine: number;
    endColumn: number;
    message: string;
    severity: string;
    helpLink?: string | null;
    file?: string | null;
}

/** Response envelope returned by the server over stdout (newline-delimited JSON). */
export interface ServerResponse {
    id: string;
    status: string;
    error?: string | null;
    problems?: ServerProblem[] | null;
}

/** Request envelope sent to the server over stdin (newline-delimited JSON). */
export interface ServerRequest {
    id: string;
    command: 'analyze' | 'shutdown';
    path?: string;
    content?: string;
    rules?: string;
    sqlVersion?: string;
    additionalAnalyzers?: string[];
}

/** Options used to launch the analyzer server process. */
export interface AnalyzerClientOptions {
    command: string;
    args: string[];
    /** Callback invoked with human-readable log lines (e.g. stderr, lifecycle). */
    log: (message: string) => void;
}

interface PendingRequest {
    resolve: (response: ServerResponse) => void;
    reject: (error: Error) => void;
}

/**
 * Manages a long-lived `SqlAnalyzerCli --server-mode` child process and exposes a
 * promise-based `analyze` call over its newline-delimited JSON protocol. The process
 * is lazily (re)spawned, so a crash simply results in a fresh process on the next request.
 */
export class AnalyzerClient {
    private child: cp.ChildProcessWithoutNullStreams | undefined;
    private reader: readline.Interface | undefined;
    private readonly pending = new Map<string, PendingRequest>();
    private requestCounter = 0;
    private disposed = false;

    constructor(private readonly options: AnalyzerClientOptions) {}

    /** Sends an analyze request and resolves with the server response. */
    public analyze(request: Omit<ServerRequest, 'id' | 'command'>): Promise<ServerResponse> {
        const id = `req-${++this.requestCounter}`;
        const payload: ServerRequest = { id, command: 'analyze', ...request };
        return this.send(payload);
    }

    /** Stops the server process, rejecting any in-flight requests. */
    public dispose(): void {
        this.disposed = true;
        this.killChild(new Error('Analyzer client disposed'));
    }

    /** Forcibly restarts the server process. */
    public restart(): void {
        this.killChild(new Error('Analyzer server restarting'));
    }

    private send(request: ServerRequest): Promise<ServerResponse> {
        if (this.disposed) {
            return Promise.reject(new Error('Analyzer client disposed'));
        }

        let child: cp.ChildProcessWithoutNullStreams;
        try {
            child = this.ensureChild();
        } catch (err) {
            return Promise.reject(err instanceof Error ? err : new Error(String(err)));
        }

        return new Promise<ServerResponse>((resolve, reject) => {
            this.pending.set(request.id, { resolve, reject });
            try {
                child.stdin.write(JSON.stringify(request) + '\n');
            } catch (err) {
                this.pending.delete(request.id);
                reject(err instanceof Error ? err : new Error(String(err)));
            }
        });
    }

    private ensureChild(): cp.ChildProcessWithoutNullStreams {
        if (this.child && !this.child.killed && this.child.exitCode === null) {
            return this.child;
        }

        const child = cp.spawn(this.options.command, this.options.args, {
            stdio: ['pipe', 'pipe', 'pipe'],
            // On Windows, `dnx`/`dotnet` may be resolved via a `.cmd` shim, which
            // Node's spawn only locates when run through a shell.
            shell: process.platform === 'win32',
        });
        this.child = child;

        this.reader = readline.createInterface({ input: child.stdout });
        this.reader.on('line', (line) => this.handleLine(line));

        child.stderr.on('data', (data: Buffer) => {
            this.options.log(data.toString().replace(/\s+$/, ''));
        });

        child.on('error', (err) => {
            this.options.log(`Analyzer process error: ${err.message}`);
            this.failAllPending(err);
            if (this.child === child) {
                this.child = undefined;
            }
        });

        child.on('exit', (code, signal) => {
            this.options.log(`Analyzer process exited (code=${code ?? 'null'}, signal=${signal ?? 'null'}).`);
            this.failAllPending(new Error('Analyzer process exited before responding'));
            if (this.child === child) {
                this.child = undefined;
            }
        });

        return child;
    }

    private handleLine(line: string): void {
        const trimmed = line.trim();
        if (!trimmed.startsWith('{')) {
            return;
        }

        let response: ServerResponse;
        try {
            response = JSON.parse(trimmed) as ServerResponse;
        } catch {
            this.options.log(`Failed to parse server response: ${trimmed}`);
            return;
        }

        const pending = this.pending.get(response.id);
        if (!pending) {
            return;
        }

        this.pending.delete(response.id);
        pending.resolve(response);
    }

    private failAllPending(error: Error): void {
        for (const [, pending] of this.pending) {
            pending.reject(error);
        }
        this.pending.clear();
    }

    private killChild(error: Error): void {
        this.failAllPending(error);
        this.reader?.close();
        this.reader = undefined;
        if (this.child) {
            try {
                this.child.kill();
            } catch {
                // ignore
            }
            this.child = undefined;
        }
    }
}
