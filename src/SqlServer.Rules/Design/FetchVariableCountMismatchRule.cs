using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>The number of variables in a FETCH statement should match the cursor's SELECT column count.</summary>
    /// <FriendlyName>FETCH variable count mismatch</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// When the number of variables in FETCH INTO does not match the number of columns in the
    /// cursor's SELECT statement, it causes a runtime error. This rule detects mismatches
    /// between cursor declaration column count and FETCH INTO variable count.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class FetchVariableCountMismatchRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0077";
        public const string RuleDisplayName = "FETCH variable count does not match cursor column count.";
        public const string Message = "FETCH INTO has {0} variable(s) but cursor '{1}' selects {2} column(s).";

        public FetchVariableCountMismatchRule()
            : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingSchemaTypes);
            if (fragment == null)
            {
                return problems;
            }

            // Collect cursor declarations and their column counts
            var cursorVisitor = new DeclareCursorStatementVisitor();
            fragment.Accept(cursorVisitor);

            var cursorColumnCounts = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);

            foreach (var cursor in cursorVisitor.Statements)
            {
                var cursorName = cursor.Name?.Value;
                if (string.IsNullOrEmpty(cursorName) || cursor.CursorDefinition?.Select == null)
                {
                    continue;
                }

                if (cursor.CursorDefinition.Select.QueryExpression is QuerySpecification querySpec)
                {
                    // Count only non-star select elements; if SELECT * is used we can't determine count
                    if (querySpec.SelectElements.OfType<SelectStarExpression>().Any())
                    {
                        continue;
                    }

                    cursorColumnCounts[cursorName] = querySpec.SelectElements.Count;
                }
            }

            if (cursorColumnCounts.Count == 0)
            {
                return problems;
            }

            // Check FETCH statements
            var fetchVisitor = new FetchStatementVisitor();
            fragment.Accept(fetchVisitor);

            foreach (var fetch in fetchVisitor.NotIgnoredStatements(RuleId))
            {
                if (fetch.IntoVariables == null || fetch.IntoVariables.Count == 0)
                {
                    continue;
                }

                var cursorName = fetch.Cursor?.Name?.Value;
                if (string.IsNullOrEmpty(cursorName))
                {
                    continue;
                }

                if (cursorColumnCounts.TryGetValue(cursorName, out var expectedColumns)
                    && fetch.IntoVariables.Count != expectedColumns)
                {
                    problems.Add(new SqlRuleProblem(
                        MessageFormatter.FormatMessage(
                            string.Format(CultureInfo.InvariantCulture, Message,
                                fetch.IntoVariables.Count, cursorName, expectedColumns),
                            RuleId),
                        sqlObj, fetch));
                }
            }

            return problems;
        }
    }
}
