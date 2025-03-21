using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>
    /// Consider replacing the OUTER JOIN with EXISTS
    /// </summary>
    /// <FriendlyName>Existence tested with JOIN</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd>
    ///   SHOULD FLAG AS PROBLEM:
    ///
    ///   ```sql
    ///       SELECT a.*
    ///       FROM a
    ///       LEFT JOIN b ON a.id = b.id
    ///       WHERE b.id IS NULL
    ///   ```
    ///
    ///   SHOULD NOT FLAG AS PROBLEM:
    ///
    ///   ```sql
    ///       SELECT a.*, b.*
    ///       FROM a
    ///       LEFT JOIN b ON a.id = b.id
    ///       WHERE b.id IS NULL
    ///   ```
    ///
    ///   ```sql
    ///       SELECT a.*, b.*
    ///       FROM a
    ///       LEFT JOIN b ON a.id = b.id
    ///   ```
    /// </ExampleMd>
    /// <remarks>
    ///   <para>Requirements:
    ///   <list type="table">
    ///     <listheader>
    ///       <term>Requirement</term>
    ///       <description>Example</description>
    ///     </listheader>
    ///     <item><term>OUTER JOIN to relation</term><description><c>FROM a LEFT OUTER JOIN b</c> or <c>FROM a RIGHT JOIN b</c></description></item>
    ///     <item><term>OUTER reference<c>b</c> <b>is not</b> referenced in SELECT </term><description> <c>SELECT a.*</c></description></item>
    ///     <item><term>OUTER reference <b>has</b> an <c>IS NULL</c> filter on JOIN member</term><description><c>WHERE b.id IS NULL</c></description></item>
    ///   </list>
    ///   </para>
    ///   <para>Description:
    ///   The traditional method of checking for row existence is to use a LEFT JOIN and checking the null-ability of a LEFT JOIN'ed column in the WHERE clause.
    ///   This method causes SQL Server to load all of the rows from the OUTER JOIN'ed table.
    ///   In cases where the matched rows are significantly less than the total rows, it is unnecessary work for SQL Server.
    ///   </para>
    ///   <para>
    ///   Alternatively checking for existence is using the EXISTS predicate function.
    ///   This is preferably to the LEFT JOIN method, since it allows SQL Server to find a row and quit(using a row count spool), avoiding unnecessary row loading.
    ///   </para>
    ///   <para>
    ///   Counter Indications:
    ///   <list type="bullet">
    ///     <item>If there are joins in the <c>EXISTS (subquery)</c>
    ///      SQL Server will favor performing loop joins through the tables, hoping to find a row quickly.
    ///      In certain cases, loop joins may be inefficient.</item>
    ///     <item>If the SQL optimizer underestimates the <c>rowcount</c> from the table in the <c>EXISTS (subquery)</c>
    ///      The query plan may show an optimal plan but the query will perform much worse.
    ///      In these cases, it is better to resort to using a LEFT JOIN and null check.</item>
    ///   </list>
    ///   </para>
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidOuterJoinsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0013";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Consider replacing the OUTER JOIN with EXISTS.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidOuterJoinsRule"/> class.
        /// </summary>
        public AvoidOuterJoinsRule()
            : base(ProgrammingAndViewSchemas)
        {
        }

        /// <summary>
        /// Performs analysis and returns a list of problems detected
        /// </summary>
        /// <param name="ruleExecutionContext">Contains the schema model and model element to analyze</param>
        /// <returns>
        /// The problems detected by the rule in the given element
        /// </returns>
        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingAndViewSchemaTypes);

            if (fragment == null)
            {
                return problems;
            }

            var visitor = new SelectStatementVisitor();
            fragment.Accept(visitor);

            foreach (var stmt in visitor.Statements)
            {
                var querySpecificationVisitor = new QuerySpecificationVisitor();
                stmt.QueryExpression.Accept(querySpecificationVisitor);

                foreach (var query in querySpecificationVisitor.Statements)
                {
                    var fromClause = query.FromClause;
                    if (fromClause == null)
                    {
                        continue;
                    }

                    var joinVisitor = new JoinVisitor();
                    fromClause.Accept(joinVisitor);

                    var outerJoins =
                        (from j in joinVisitor.QualifiedJoins
                         let t = j.QualifiedJoinType
                         where (t == QualifiedJoinType.LeftOuter || t == QualifiedJoinType.RightOuter)
                            && Ignorables.ShouldNotIgnoreRule(j.ScriptTokenStream, RuleId, j.StartLine)
                         select j).ToList();

                    if (outerJoins.Count == 0)
                    {
                        continue;
                    }

                    var columns = ColumnReferenceExpressionVisitor.VisitSelectElements(query.SelectElements);

                    var whereClause = query.WhereClause;
                    if (whereClause == null)
                    {
                        continue;
                    }

                    var isnullVisitor = new ISNULLVisitor();
                    whereClause.Accept(isnullVisitor);
                    if (isnullVisitor.Count == 0)
                    {
                        continue;
                    }

                    foreach (var join in outerJoins)
                    {
                        TableReference table;
                        if (join.QualifiedJoinType == QualifiedJoinType.LeftOuter)
                        {
                            table = join.SecondTableReference as TableReference;
                        }
                        else
                        {
                            table = join.FirstTableReference as TableReference;
                        }

                        var tableName = table.GetName();
                        var alias = (table as TableReferenceWithAlias)?.Alias.Value;

                        // are there any columns in the select that match this table?
                        if (columns.Any(c =>
                        {
                            var colTableName = c.GetName();
                            return Comparer.Equals(tableName, colTableName) || Comparer.Equals(alias, colTableName);
                        }))
                        {
                            continue;
                        }

                        // no columns, now we need to look in the where clause for a null check against this table.
                        if (isnullVisitor.Statements.Any(nc =>
                        {
                            var col = nc.FirstExpression as ColumnReferenceExpression ?? nc.SecondExpression as ColumnReferenceExpression;
                            if (col == null)
                            {
                                return false;
                            }

                            var colTableName = col.GetName();
                            return Comparer.Equals(tableName, colTableName) || Comparer.Equals(alias, colTableName);
                        }))
                        {
                            problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, join));
                        }
                    }
                }
            }

            return problems;
        }
    }
}