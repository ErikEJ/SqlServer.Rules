using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>Hard-coded credentials are a security risk.</summary>
    /// <FriendlyName>Hard-coded credentials</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Variables or parameters with names containing 'password', 'pwd', 'secret', or 'apikey'
    /// should not be assigned string literal values. Credentials should be retrieved from
    /// secure configuration or secret stores at runtime.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class HardCodedCredentialsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0075";
        public const string RuleDisplayName = "Avoid hard-coded credentials. Use secure configuration instead.";
        public const string Message = "Variable '{0}' appears to contain a hard-coded credential.";

        private static readonly Regex CredentialPattern = new Regex(
            @"(password|passwd|pwd|secret|apikey|api_key|token|credential)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public HardCodedCredentialsRule()
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

            var visitor = new SetVariableStatementVisitor();
            fragment.Accept(visitor);

            foreach (var stmt in visitor.NotIgnoredStatements(RuleId))
            {
                if (stmt.Expression is StringLiteral literal
                    && !string.IsNullOrEmpty(literal.Value)
                    && CredentialPattern.IsMatch(stmt.Variable.Name))
                {
                    problems.Add(new SqlRuleProblem(
                        MessageFormatter.FormatMessage(
                            string.Format(CultureInfo.InvariantCulture, Message, stmt.Variable.Name),
                            RuleId),
                        sqlObj, stmt));
                }
            }

            // Also check DECLARE @var type = 'literal'
            var declareVisitor = new DeclareVariableElementVisitor();
            fragment.Accept(declareVisitor);

            foreach (var decl in declareVisitor.NotIgnoredStatements(RuleId))
            {
                if (decl.Value is StringLiteral declLiteral
                    && !string.IsNullOrEmpty(declLiteral.Value)
                    && CredentialPattern.IsMatch(decl.VariableName.Value))
                {
                    problems.Add(new SqlRuleProblem(
                        MessageFormatter.FormatMessage(
                            string.Format(CultureInfo.InvariantCulture, Message, decl.VariableName.Value),
                            RuleId),
                        sqlObj, decl));
                }
            }

            return problems;
        }
    }
}
