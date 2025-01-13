using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>Index has exact duplicate or overlapping index. Combine indexes to reduce over-head </summary>
    /// <FriendlyName>Duplicate/Overlapping Index</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule matches exact duplicating or partially duplicating indexes. The exact duplicating
    /// indexes must have the same key columns in the same order, and the same included columns but
    /// in any order. These indexes are sure targets for elimination. The overlapping indexes share
    /// the same leading key columns, but the included columns are ignored. These types of indexes
    /// are probable dead indexes walking.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class DuplicateIndexesRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0052";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Index has exact duplicate or borderline overlapping index.";

        /// <summary>
        /// The message duplicate
        /// </summary>
        public const string MessageDuplicate = "'{0}' is a duplicate index.";

        /// <summary>
        /// The message border line
        /// </summary>
        public const string MessageBorderLine = "'{0}' is a borderline duplicate index.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateIndexesRule"/> class.
        /// </summary>
        public DuplicateIndexesRule() : base(ModelSchema.Table)
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

            var objName = sqlObj.Name.GetName();

            var indexes = sqlObj.GetReferencing(DacQueryScopes.All)
                .Where(x => x.ObjectType == Index.TypeClass).Select(x => x.GetFragment())
                .ToList();

            if (indexes.Count == 0) { return problems; }

            var indexVisitor = new CreateIndexStatementVisitor();
            foreach (var index in indexes)
            {
                index?.Accept(indexVisitor);
            }

            var indexInfo = new Dictionary<CreateIndexStatement, List<string>>();
            foreach (var index in indexVisitor.Statements)
            {
#pragma warning disable CA1308 // Normalize strings to uppercase
                indexInfo.Add(index, new List<string>(index.Columns.Select(col => col.Column.GetName().ToLower(CultureInfo.InvariantCulture))));
#pragma warning restore CA1308 // Normalize strings to uppercase
            }

            if (indexInfo.Count == 0) { return problems; }

            // find all the duplicates where all the columns match
            var dupes = indexInfo.GroupBy(x => string.Join(",", x.Value))
                .Where(x => x.Count() > 1).SelectMany(x => x).ToList();
            problems.AddRange(dupes
                .Select(ix => new SqlRuleProblem(string.Format(CultureInfo.InvariantCulture, MessageDuplicate, ix.Key.Name.Value), sqlObj, ix.Key)));

            // remove the exact duplicates to try to search for border line duplicates
            indexInfo.RemoveAll((key, value) => dupes.Any(x => x.Key == key));

            if (indexInfo.Count <= 1) { return problems; }

            // find all the borderline duplicates where the first column matches
            var borderLineDupes = indexInfo.GroupBy(x => x.Value.First()).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
            problems.AddRange(borderLineDupes
                .Select(ix => new SqlRuleProblem(string.Format(CultureInfo.InvariantCulture, MessageBorderLine, ix.Key.Name.Value), sqlObj, ix.Key)));

            return problems;
        }
    }
}