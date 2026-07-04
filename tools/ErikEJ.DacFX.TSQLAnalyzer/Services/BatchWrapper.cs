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
    public static string Wrap(string sql) => WrapWithAdjustments(sql).WrappedSql;

    /// <summary>
    /// Returns <paramref name="sql"/> with every wrappable ad-hoc batch enclosed in a synthetic
    /// stored procedure, along with column-offset adjustments needed to map wrapped or normalized
    /// positions back to the original source.
    /// </summary>
    /// <param name="sql">The T-SQL script to process.</param>
    /// <returns>The transformed SQL and the column adjustments needed to map results back to the original source.</returns>
    internal static (string WrappedSql, IReadOnlyList<ColumnAdjustment> Adjustments) WrapWithAdjustments(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return (sql, []);
        }

        var script = TryParse(sql);

        if (script == null)
        {
            return (sql, []);
        }

        // Collect the edits first, then apply them right-to-left so earlier offsets stay valid.
        var edits = new List<(int Offset, int Length, string Text)>();
        var adjustments = new List<ColumnAdjustment>();
        var index = 0;

        foreach (var batch in script.Batches)
        {
            if (batch.Statements.Count == 0)
            {
                continue;
            }

            if (TryNormalizeAlterObjectDefinition(sql, batch, edits, adjustments))
            {
                continue;
            }

            if (!IsWrappable(batch))
            {
                continue;
            }

            index++;
            var name = string.Create(CultureInfo.InvariantCulture, $"[dbo].[{SyntheticObjectPrefix}{index}]");
            var prefix = $"CREATE PROCEDURE {name} AS BEGIN ";

            edits.Add((batch.StartOffset, 0, prefix));
            edits.Add((batch.StartOffset + batch.FragmentLength, 0, " END;"));

            // Any token on batch.StartLine is shifted right by the prefix length.
            adjustments.Add(new ColumnAdjustment(batch.StartLine, 1, prefix.Length));
        }

        if (edits.Count == 0)
        {
            return (sql, []);
        }

        var builder = new StringBuilder(sql);

        foreach (var (offset, length, text) in edits.OrderByDescending(e => e.Offset))
        {
            builder.Remove(offset, length);
            builder.Insert(offset, text);
        }

        return (builder.ToString(), adjustments.AsReadOnly());
    }

    private static bool TryNormalizeAlterObjectDefinition(
        string sql,
        TSqlBatch batch,
        List<(int Offset, int Length, string Text)> edits,
        List<ColumnAdjustment> adjustments)
    {
        if (batch.Statements.Count != 1)
        {
            return false;
        }

        var statement = batch.Statements[0];

        if (IsCreateOrAlterDefinition(statement))
        {
            var tokens = GetStatementTokens(statement).ToList();
            var orToken = tokens.FirstOrDefault(t => t.TokenType == TSqlTokenType.Or);
            var alterToken = tokens.FirstOrDefault(t => t.TokenType == TSqlTokenType.Alter);

            if (orToken == null || alterToken == null || alterToken.Offset < orToken.Offset)
            {
                return false;
            }

            var offset = orToken.Offset;
            var length = (alterToken.Offset + alterToken.Text.Length) - offset;
            edits.Add((offset, length, CreateWhitespacePreservingReplacement(sql, offset, length)));
            return true;
        }

        if (!IsAlterDefinition(statement))
        {
            return false;
        }

        var alter = GetStatementTokens(statement).FirstOrDefault(t => t.TokenType == TSqlTokenType.Alter);
        if (alter == null)
        {
            return false;
        }

        const string createKeyword = "CREATE";
        edits.Add((alter.Offset, alter.Text.Length, createKeyword));
        adjustments.Add(new ColumnAdjustment(alter.Line, alter.Column + createKeyword.Length, createKeyword.Length - alter.Text.Length));
        return true;
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

    private static bool IsAlterDefinition(TSqlStatement statement) => statement switch
    {
        AlterProcedureStatement => true,
        AlterFunctionStatement => true,
        AlterViewStatement => true,
        AlterTriggerStatement => true,
        _ => false,
    };

    private static bool IsCreateOrAlterDefinition(TSqlStatement statement) => statement switch
    {
        CreateOrAlterProcedureStatement => true,
        CreateOrAlterFunctionStatement => true,
        CreateOrAlterViewStatement => true,
        CreateOrAlterTriggerStatement => true,
        _ => false,
    };

    private static IEnumerable<TSqlParserToken> GetStatementTokens(TSqlStatement statement)
        => statement.ScriptTokenStream.Where(
            token => token.Offset >= statement.StartOffset && token.Offset < statement.StartOffset + statement.FragmentLength);

    private static string CreateWhitespacePreservingReplacement(string sql, int offset, int length)
    {
        var replacement = sql.Substring(offset, length).ToCharArray();

        for (var i = 0; i < replacement.Length; i++)
        {
            if (replacement[i] != '\r' && replacement[i] != '\n')
            {
                replacement[i] = ' ';
            }
        }

        return new string(replacement);
    }

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
