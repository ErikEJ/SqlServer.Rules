using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using SqlServer.Rules.ReferentialIntegrity;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Make sure you have all the columns for the JOIN relationship as conditions in the ON clause
    /// </summary>
    /// <FriendlyName>Incomplete or missing JOIN predicate</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// <list type="bullet">
    /// <item>It is either missing a backing foreign key or the join is missing one or more columns.
    /// The rule checks the T-SQL code for queries having joined tables and missing join a predicate
    /// for one of the tables. </item>
    /// <item>It identifies the joined table sources which do not have any column
    /// referenced neither in the join conditions nor in the WHERE clause. Without a join predicate,
    /// the query result will include the Cartesian product of all rows.</item>
    /// </list>
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class MissingJoinPredicateRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0020";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "The query has issues with the join clause. It is either missing a backing foreign key or the join is missing one or more columns.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "The query is missing one or more columns in the join clause. This may affect the results or may result in an unexpected row difference.";

        /// <summary>
        /// The message no join
        /// </summary>
        public const string MessageNoJoin = "The query is using a join, but there is no backing foreign key to match the join.";

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingJoinPredicateRule"/> class.
        /// </summary>
        public MissingJoinPredicateRule() : base(ProgrammingAndViewSchemas)
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
            var db = ruleExecutionContext.SchemaModel;

            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

            var fromClauseVisitor = new FromClauseVisitor();
            fragment.Accept(fromClauseVisitor);

            if (fromClauseVisitor.Count == 0) { return problems; }

            // cache all the fks we find 
            var fkList = new Dictionary<string, ForeignKeyInfo>();

            foreach (var from in fromClauseVisitor.Statements.Where(x => x.TableReferences.First() is QualifiedJoin))
            {
                var joinInfos = from.GetFromClauseJoinTables()
                    .Where(x => x.Table1JoinColumns.Count > 0 && (x.Table1JoinColumns.Count == x.Table2JoinColumns.Count)
                ).ToList();
                if (joinInfos.Count == 0) { continue; }

                foreach (var join in joinInfos)
                {
                    if (join.Table1 == null || join.Table2 == null) { continue; }

                    // get the tables belonging to this join
                    var table1 = db.GetObject(Table.TypeClass, join.Table1Name, DacQueryScopes.All);
                    var table2 = db.GetObject(Table.TypeClass, join.Table2Name, DacQueryScopes.All);

                    // this can happen when one of the tables is a temp table, in that case we do not care to inspect the fks
                    if (table1 == null || table2 == null) { continue; }

                    fkList.AddRange(table1.GetTableFKInfos());

                    // only get the fks for table2 if it is a diff table, otherwise it is a self join
                    if (table1.Name.CompareTo(table2.Name) < 5)
                    {
                        fkList.AddRange(table2.GetTableFKInfos());
                    }

                    // find any possible fks where the table names match exactly
                    var possibleFks = fkList.Where(f => join.CheckTableNames(f.Value)).ToList();

                    // we did not find any fks where the tables in the join matched the tables in the fk
                    if (possibleFks.Count == 0)
                    {
                        problems.Add(new SqlRuleProblem(MessageNoJoin, sqlObj, join.Table2));
                        continue;
                    }

                    // check to see if all of the columns match
                    if (!possibleFks.Any(f => join.CheckFullJoin(f.Value)))
                    {
                        problems.Add(new SqlRuleProblem(Message, sqlObj, join.Table2));
                    }
                }
            }

            return problems;
        }
    }
}