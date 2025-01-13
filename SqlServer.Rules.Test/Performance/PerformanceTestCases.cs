using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Performance;

namespace SqlServer.Rules.Tests.Performance
{
    [TestClass]
    [TestCategory("Performance")]
    public class PerformanceTestCases : TestCasesBase
    {
        [TestMethod]
        public void TestNonSARGablePattern()
        {
            var problems = GetTestCaseProblems(nameof(AvoidEndsWithOrContainsRule), AvoidEndsWithOrContainsRule.RuleId);

            Assert.AreEqual(2, problems.Count, "Expected 2 problem to be found");

            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "nonsargable.sql")));
            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "nonsargable2.sql")));

            Assert.IsTrue(problems.All(problem => Comparer.Equals(problem.Description, AvoidEndsWithOrContainsRule.Message)));
            Assert.IsTrue(problems.All(problem => problem.Severity == SqlRuleProblemSeverity.Warning));
        }

        [TestMethod]
        public void TestNotEqualToRule()
        {
            var problems = GetTestCaseProblems(nameof(AvoidNotEqualToRule), AvoidNotEqualToRule.RuleId);

            Assert.AreEqual(2, problems.Count, "Expected 2 problem to be found");

            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "ansi_not_equal.sql")));
            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "alternate_not_equal.sql")));

            Assert.IsTrue(problems.All(problem => Comparer.Equals(problem.Description, AvoidNotEqualToRule.Message)));
            Assert.IsTrue(problems.All(problem => problem.Severity == SqlRuleProblemSeverity.Warning));
        }

        [TestMethod]
        public void TestAvoidCalcOnColumn()
        {
            var problems = GetTestCaseProblems(nameof(AvoidColumnCalcsRule), AvoidColumnCalcsRule.RuleId);

            Assert.AreEqual(1, problems.Count, "Expected 1 problem to be found");

            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "calc_on_column.sql")));

            Assert.IsTrue(problems.All(problem => Comparer.Equals(problem.Description, AvoidColumnCalcsRule.Message)));
            Assert.IsTrue(problems.All(problem => problem.Severity == SqlRuleProblemSeverity.Warning));
        }

        [TestMethod]
        public void TestAvoidColumnFunctionsRule()
        {
            var problems = GetTestCaseProblems(nameof(AvoidColumnFunctionsRule), AvoidColumnFunctionsRule.RuleId);

            Assert.AreEqual(1, problems.Count, "Expected 1 problem to be found");

            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "func_on_column.sql")));

            Assert.IsTrue(problems.All(problem => Comparer.Equals(problem.Description, AvoidColumnFunctionsRule.Message)));
            Assert.IsTrue(problems.All(problem => problem.Severity == SqlRuleProblemSeverity.Warning));
        }
    }
}
