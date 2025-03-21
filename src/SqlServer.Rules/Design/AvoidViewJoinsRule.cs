using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Avoid joining tables with views.
    /// </summary>
    /// <FriendlyName>Avoid joining tables with views</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidViewJoinsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0019";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid joining tables with views.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidViewJoinsRule"/> class.
        /// </summary>
        public AvoidViewJoinsRule()
            : base(ProgrammingSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingSchemaTypes);

            if (fragment == null)
            {
                return problems;
            }

            var visitor = new JoinVisitor();

            fragment.Accept(visitor);
            var views = sqlObj.GetReferenced(DacQueryScopes.UserDefined)
                .Where(x => x.ObjectType == ModelSchema.View).Select(v => v.Name.Parts.Last()).ToList();

            var joins = visitor.QualifiedJoins.Where(j => Ignorables.ShouldNotIgnoreRule(j.ScriptTokenStream, RuleId, j.StartLine));

            var leftSideOffenders =
                from o in joins
                where o.FirstTableReference != null
                    && o.FirstTableReference is NamedTableReference
                    && views.Contains((o.FirstTableReference as NamedTableReference)!.SchemaObject.Identifiers.Last().Value)
                select o.FirstTableReference as NamedTableReference;

            var rightSideOffenders =
                from o in joins
                where o.SecondTableReference != null
                    && o.SecondTableReference is NamedTableReference
                    && views.Contains((o.SecondTableReference as NamedTableReference)!.SchemaObject.Identifiers.Last().Value)
                select o.SecondTableReference as NamedTableReference;

            problems.AddRange(leftSideOffenders.Union(rightSideOffenders).Select(o => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, o)));

            return problems;
        }
    }
}