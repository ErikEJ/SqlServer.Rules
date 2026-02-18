using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>Weak hashing algorithms (MD2, MD4, MD5, SHA, SHA1) should not be used.</summary>
    /// <FriendlyName>Weak hashing algorithm</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// HASHBYTES with MD2, MD4, MD5, SHA, or SHA1 uses weak hashing algorithms that are
    /// vulnerable to collision attacks. Use SHA2_256 or SHA2_512 instead.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class WeakHashingAlgorithmRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0074";
        public const string RuleDisplayName = "Avoid weak hashing algorithms (MD2, MD4, MD5, SHA, SHA1). Use SHA2_256 or SHA2_512.";
        public const string Message = "Weak hashing algorithm '{0}' used in HASHBYTES. Use SHA2_256 or SHA2_512 instead.";

        private static readonly HashSet<string> WeakAlgorithms = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "MD2", "MD4", "MD5", "SHA", "SHA1",
        };

        public WeakHashingAlgorithmRule()
            : base(ProgrammingAndViewSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingAndViewSchemaTypes);
            if (fragment == null)
            {
                return problems;
            }

            var visitor = new FunctionCallVisitor("HASHBYTES");
            fragment.Accept(visitor);

            foreach (var call in visitor.NotIgnoredStatements(RuleId))
            {
                if (call.Parameters.Count >= 1
                    && call.Parameters[0] is StringLiteral algorithmLiteral)
                {
                    var algorithm = algorithmLiteral.Value;
                    if (WeakAlgorithms.Contains(algorithm))
                    {
                        problems.Add(new SqlRuleProblem(
                            MessageFormatter.FormatMessage(
                                string.Format(System.Globalization.CultureInfo.InvariantCulture, Message, algorithm),
                                RuleId),
                            sqlObj, call));
                    }
                }
            }

            return problems;
        }
    }
}
