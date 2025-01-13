using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;

namespace SqlServer.Rules.Report
{
    public static class ProblemExtensions
    {
        public static string Rule(this SqlRuleProblem problem) => problem.RuleId.Split('.').Last();
    }
}