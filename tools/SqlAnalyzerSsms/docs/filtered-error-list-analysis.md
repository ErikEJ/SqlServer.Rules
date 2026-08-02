# Analysis: SSMS "Filtered" Error List is empty (#635)

> This is an **analysis-only** document. It explains *why* the Error List can appear
> empty even though squiggles and the status-bar warning/error counts are present.
> No code is changed by this document.

## Symptom

Reported in [#635](https://github.com/ErikEJ/SqlServer.Rules/issues/635):

1. Open a new query window in SSMS and connect it.
2. Type something like `select * from table`.
3. Rule violations show up correctly:
   - green/blue **squiggles** appear under the offending tokens, and
   - the editor status bar shows the aggregate counts (e.g. `0` errors, `5` warnings).
4. **Opening/clicking into the Error List shows an empty list.**

The maintainer also notes the same problem happens for **syntax errors**, i.e. it is not
specific to any particular rule.

The important observation is that the diagnostics *do* exist — the squiggles (from the
tagger) and the aggregate counts (from the Error List table entries) prove that. So the
diagnostics are being produced and pushed into the Error List table. What fails is the
**filtering/matching** that the Error List applies before rendering the rows.

## How the two rendering paths differ

There are two independent consumers of the analysis results, and only one of them is
broken:

| Path | Source file | Uses document identity? |
| --- | --- | --- |
| Editor squiggles | `Linter/Tagging/SqlLintTagger.cs` | No — tags are attached directly to the `ITextBuffer`/`SnapshotSpan`, so they render regardless of any file/document name. |
| Error List rows | `Linter/ErrorList/*` | **Yes** — each row carries `DocumentName` / `Path`, and the Error List filters rows by document. |

Because the squiggle path never depends on a document name, it always works. The Error
List path does depend on the document name, and that is where the mismatch happens.

## Where the document name comes from

The Error List row content is produced here:

- `Linter/ErrorList/DocumentHandler.cs` → on each analysis update calls
  `DocumentIdentity.GetDocumentName(_textView)` and then
  `_tableDataSource.UpdateErrors(_filePath, documentName, e.ProjectName, e.Violations)`.
- `Linter/ErrorList/SqlLintTableDataSource.cs` → builds a `SqlLintError` per violation.
- `Linter/ErrorList/SqlLintError.cs` → stores `FilePath` (full path) and `DocumentName`.
- `Linter/ErrorList/TableEntriesSnapshot.cs` → exposes those to the table via
  `StandardTableKeyNames.DocumentName` (returns `error.DocumentName`) and
  `StandardTableKeyNames.Path` (returns `error.FilePath`).

Crucially, `DocumentIdentity.GetDocumentName(...)`
(`Linter/ErrorList/DocumentIdentity.cs`) returns a **short / display name**, not the full
document moniker:

- For an unsaved SSMS query window it returns the *virtual caption* like `SQLQuery1.sql`
  (parsed from the window caption via the regex `^(?<name>SQLQuery\d+\.sql)`), or the
  fallback `query.sql`.
- For a saved file it returns `Path.GetFileName(filePath)` (e.g. `MyScript.sql`).

So the value stored in `StandardTableKeyNames.DocumentName` is a **bare file name**, while
`StandardTableKeyNames.Path` holds the **full path** (`_filePath`, which is
`ITextDocument.FilePath`).

## Root cause: the Error List "Current Document" scope filter matches on `DocumentName`, and it expects the document *moniker* (full path)

The Visual Studio (and therefore SSMS) Error List has a scope filter — "Entire
Solution / Open Documents / Current Project / **Current Document**". When the scope is
"Current Document" (and, depending on shell/version, this can be the effective scope for an
editor that is not part of a loaded solution/project), the shell:

1. Determines the **active document's moniker** — the full document path that the Running
   Document Table (RDT) knows the window by (`VSFPROPID_pszMkDocument`). For an SSMS query
   window this is the *actual* backing file path (an unsaved query is backed by a temp file
   under `%TEMP%`, e.g. `C:\Users\...\AppData\Local\Temp\~vsXXXX.sql`), **not** the
   `SQLQuery1.sql` caption shown on the tab.
2. Filters the table rows by comparing each row's `StandardTableKeyNames.DocumentName`
   value against that moniker.

Our rows publish `DocumentName = "SQLQuery1.sql"` (or `"query.sql"`, or a bare file name),
which never equals the active document's full moniker. **The comparison fails for every
row, so the filtered Error List is empty**, even though the rows exist (hence the non-zero
status-bar counts) and the squiggles are drawn.

This also explains why the problem is not rule-specific and reproduces for **syntax
errors** too: it is a document-identity/filter mismatch, independent of which diagnostic
produced the row.

### Why the full path lives in the wrong key

The two standard keys have distinct, well-defined meanings for the Error List:

- `StandardTableKeyNames.DocumentName` — the **full path (moniker)** of the file the entry
  belongs to. This is what the Error List uses for (a) the "Current Document" scope filter
  and (b) double-click navigation.
- `StandardTableKeyNames.Path` — an auxiliary, display-oriented path column.

The current code has these effectively swapped for filtering purposes: the full path is put
in `Path` (`error.FilePath`) while `DocumentName` gets a short/virtual name. Navigation may
still work in some cases because the code also supplies `StandardTableKeyNames.Path`, `Line`
and `Column`, but the **scope filter keys off `DocumentName`** and therefore fails.

## Why previous fix attempts "broke everything"

The maintainer comments that trying to get the "virtual" file name broke things previously.
That is consistent with the analysis above:

- SSMS unsaved query windows have **two** identities:
  - the **display caption** (`SQLQuery1.sql`) shown on the tab, and
  - the **backing moniker** (a `%TEMP%\~vs*.sql` path) used by the RDT and by
    `ITextDocument.FilePath`.
- The analyzer/tagger pipeline keys everything off `ITextDocument.FilePath` (the temp
  moniker) — see `SqlLintTagger.GetFilePath()` and
  `SqlDocumentListener.GetFilePath()`. That is correct and consistent.
- `DocumentIdentity` was introduced to make the Error List show a *friendly* name
  (`SQLQuery1.sql`) instead of an ugly temp path. But by putting that friendly name into
  `DocumentName`, it also became the value the **filter** compares against — and the filter
  needs the *moniker*, not the friendly caption. Making the display "nicer" silently broke
  the filter match.

In other words, display friendliness and filter correctness were coupled through the single
`DocumentName` key, and optimizing one regressed the other.

There is already diagnostic logging in place to confirm this at runtime
(`DocumentHandler.OnAnalysisUpdated` logs
`Identity: filePath='...', documentName='...'`). Comparing the logged `documentName`
against the active document moniker (from the RDT / `VSFPROPID_pszMkDocument`) in a repro
will show them differing — the temp path vs. `SQLQuery1.sql`.

## Recommended direction for a fix (not applied here)

The safe invariant is: **the value published under
`StandardTableKeyNames.DocumentName` must be exactly the document moniker that the shell
uses for the active window** — i.e. the same string as `ITextDocument.FilePath` /
`VSFPROPID_pszMkDocument`. Concretely:

1. Publish `DocumentName = _filePath` (the actual moniker used everywhere else in the
   pipeline) instead of the short/virtual caption. This makes the "Current Document" scope
   filter match and the list populate.
2. Keep a friendly display separately: use `StandardTableKeyNames.Path` (or a dedicated
   display column) for the human-friendly `SQLQuery1.sql`, so the UI still looks clean
   without affecting filtering or navigation.
3. If the friendly name must drive the moniker (e.g. a genuinely file-less buffer), obtain
   the moniker authoritatively from the RDT via `VSFPROPID_pszMkDocument` for the active
   window frame rather than reconstructing it from the tab caption, and use that identical
   string for both the row's `DocumentName` and any navigation.
4. Verify with the existing `DocumentHandler` log that `documentName` equals the active
   document moniker for: (a) an unsaved query window, (b) a saved `.sql` file, and (c) a
   `.sql` file that belongs to a loaded SQL project.

The key takeaway: **`DocumentName` is a filter/navigation key, not a display string.** It
must equal the shell's document moniker, and any friendlier text belongs in a separate
display key.
