using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;

namespace TSQLSmellSCA
{
    [LocalizedExportCodeAnalysisRule(
        RuleId,
        RuleConstants.ResourceBaseName, // Name of the resource file to look up displayname and description in
        RuleConstants.TSQLSmell_RuleName19, // ID used to look up the display name inside the resources file
        null,
        Category = RuleConstants.CategorySmells,
        RuleScope = SqlRuleScope.Model)] // This rule targets the whole model
    public sealed class TSQLSmellSCA19 : SqlCodeAnalysisRule
    {
        public const string RuleId = "Smells.SML019";

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var Worker = new TSQLSmellWorker(ruleExecutionContext, RuleId);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            return Worker.Analyze();
        }
    }
}
