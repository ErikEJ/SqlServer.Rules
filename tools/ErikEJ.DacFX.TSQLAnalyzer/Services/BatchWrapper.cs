using System.Globalization;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace ErikEJ.DacFX.TSQLAnalyzer.Services;

/// <summary>
/// Wraps ad-hoc (non object-creation) T-SQL batches in throw-away stored procedures so that the
/// model-based DacFx code-analysis rules, which only run against schema objects, can be applied
/// to scripts such as migration, deployment and investigation scripts.
/// </summary>
/// <remarks>
/// The wrapper text is inserted inline (without adding any new lines) at the start and end of each
/// batch, so every original line keeps its number and reported problems still point at the real
/// source line. Batches that already are object definitions, or that contain statements that are
/// illegal inside a procedure body, are left untouched.
/// </remarks>
internal sealed class BatchWrapper
{
    /// <summary>
    /// Object name prefix used for the generated procedures. Problem suppression matches on this
    /// prefix to drop noise (e.g. naming violations) raised against the synthetic objects, so keep
    /// the two in sync.
    /// </summary>
    public const string SyntheticObjectPrefix = "__tsqlanalyzer_adhoc_batch_";

    /// <summary>
    /// Returns <paramref name="sql"/> with every wrappable ad-hoc batch enclosed in a synthetic
    /// stored procedure. If the script cannot be parsed, or contains nothing to wrap, the original
    /// text is returned unchanged.
    /// </summary>
    /// <param name="sql">The T-SQL script to process.</param>
    /// <returns>The script with ad-hoc batches wrapped, or the original text when nothing was wrapped.</returns>
    public static string Wrap(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return sql;
        }

        var script = TryParse(sql);

        if (script == null)
        {
            return sql;
        }

        // Collect the edits first, then apply them right-to-left so earlier offsets stay valid.
        var edits = new List<(int Offset, string Text)>();
        var index = 0;

        foreach (var batch in script.Batches)
        {
            if (batch.Statements.Count == 0 || !IsWrappable(batch))
            {
                continue;
            }

            index++;
            var name = string.Create(CultureInfo.InvariantCulture, $"[dbo].[{SyntheticObjectPrefix}{index}]");

            edits.Add((batch.StartOffset, $"CREATE PROCEDURE {name} AS BEGIN "));
            edits.Add((batch.StartOffset + batch.FragmentLength, " END;"));
        }

        if (edits.Count == 0)
        {
            return sql;
        }

        var builder = new StringBuilder(sql);

        foreach (var (offset, text) in edits.OrderByDescending(e => e.Offset))
        {
            builder.Insert(offset, text);
        }

        return builder.ToString();
    }

    private static bool IsWrappable(TSqlBatch batch)
    {
        foreach (var statement in batch.Statements)
        {
            if (!IsWrappableStatement(statement))
            {
                return false;
            }
        }

        return true;
    }

    // Allow-list of statements that are both valuable to analyze and legal inside a procedure body.
    // Anything outside this set (object definitions, batch-only statements like USE / ALTER
    // DATABASE, etc.) leaves the batch untouched so it is added to the model as-is.
    private static bool IsWrappableStatement(TSqlStatement statement) => statement switch
    {
        SelectStatement => true,
        InsertStatement => true,
        UpdateStatement => true,
        DeleteStatement => true,
        MergeStatement => true,
        ExecuteStatement => true,
        DeclareVariableStatement => true,
        DeclareTableVariableStatement => true,
        SetVariableStatement => true,
        IfStatement => true,
        WhileStatement => true,
        BeginEndBlockStatement => true,
        TryCatchStatement => true,
        PrintStatement => true,
        ThrowStatement => true,
        RaiseErrorStatement => true,
        WaitForStatement => true,
        DeclareCursorStatement => true,
        OpenCursorStatement => true,
        FetchCursorStatement => true,
        CloseCursorStatement => true,
        DeallocateCursorStatement => true,
        BeginTransactionStatement => true,
        CommitTransactionStatement => true,
        RollbackTransactionStatement => true,
        SaveTransactionStatement => true,
        _ => false,
    };

    private static TSqlScript? TryParse(string sql)
    {
        try
        {
            // Use the newest parser and all engine types to support every SQL Server version.
            var parser = new TSql170Parser(true, SqlEngineType.All);

            using var reader = new StringReader(sql);
            var fragment = parser.Parse(reader, out IList<ParseError> errors);

            if (errors.Count > 0)
            {
                return null;
            }

            return fragment as TSqlScript;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return null;
        }
    }
}
