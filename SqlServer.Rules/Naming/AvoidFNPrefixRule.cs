using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Naming
{
    /// <summary>
    /// Function name may conflict system name. Avoid 'fn_' prefix when naming functions.
    /// </summary>
    /// <FriendlyName>UDF with System prefix</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// This rule checks for user defined scalar functions with 'fn_'. Though this practice is
    /// supported, it is recommended that the prefixes not be used to avoid name clashes with
    /// Microsoft shipped objects.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.Naming.NamingViolationRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Naming,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidFNPrefixRule : NamingViolationRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRN0001";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid 'fn_' prefix when naming functions.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidFNPrefixRule"/> class.
        /// </summary>
        public AvoidFNPrefixRule()
            : base(
            RuleId,
            RuleDisplayName,
            "fn_",
            new[] { ModelSchema.PartitionFunction, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction },
            n => n.StartsWith)
        {
        }
    }
}