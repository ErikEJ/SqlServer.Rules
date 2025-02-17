using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Naming
{
    /// <summary>
    /// Avoid 'sp_' prefix when naming stored procedures.
    /// </summary>
    /// <FriendlyName>Procedure name may conflict system name</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// This rule checks for creation of stored procedure with names starting with `sp_`.The prefix
    /// `sp_` is reserved for system stored procedure that ship with SQL Server. Whenever SQL Server
    /// encounters a procedure name starting with sp_, it first tries to locate the procedure in the
    /// master database, then it looks for any qualifiers (database, owner) provided, then it tries
    /// dbo as the owner. So you can really save time in locating the stored procedure by avoiding
    /// the `sp_` prefix.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.Naming.NamingViolationRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Naming,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidSPPrefixRule : NamingViolationRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRN0002";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid 'sp_' prefix when naming stored procedures.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidSPPrefixRule"/> class.
        /// </summary>
        public AvoidSPPrefixRule()
            : base(
            RuleId,
            MessageFormatter.FormatMessage(RuleDisplayName, RuleId),
            "sp_",
            new[] { ModelSchema.Procedure },
            n => n.StartsWith)
        {
        }
    }
}