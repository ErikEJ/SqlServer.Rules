using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
[TestCategory("Design")]
public class DesignTestCases : TestCasesBase
{
    [TestMethod]
    public void TestAvoidNotForReplication()
    {
        var problems = GetTestCaseProblems(nameof(NotForReplication), NotForReplication.RuleId);

        const int expected = 4;
        Assert.HasCount(expected, problems, $"Expected {expected} problem(s) to be found");

        Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "dbo_table2_trigger_1_not_for_replication.sql")));
        Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "fk_table2_table1_1_not_for_replication.sql")));
        Assert.AreEqual(2, problems.Count(problem => Comparer.Equals(problem.SourceName, "table3.sql")));

        Assert.IsTrue(problems.All(problem => problem.Description.StartsWith(NotForReplication.Message, System.StringComparison.Ordinal)));
        Assert.IsTrue(problems.All(problem => problem.Severity == SqlRuleProblemSeverity.Warning));
    }

    [TestMethod]
    public void TestMissingJoinPredicateFalsePositive()
    {
        var problems = GetTestCaseProblems(nameof(MissingJoinPredicateRule), MissingJoinPredicateRule.RuleId);

        const int expected = 1;
        Assert.HasCount(expected, problems, $"Expected {expected} problem(s) to be found");

        Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "mtgfunc.sql")));

        Assert.IsTrue(problems.All(problem => Comparer.Equals(problem.Description, MissingJoinPredicateRule.MessageNoJoin)));
        Assert.IsTrue(problems.All(problem => problem.Severity == SqlRuleProblemSeverity.Warning));
    }

    [TestMethod]
    public void TestTableMissingClusteredIndexRule()
    {
        var problems = GetTestCaseProblems(nameof(TableMissingClusteredIndexRule), TableMissingClusteredIndexRule.RuleId);

        Assert.IsEmpty(problems, "Expected 0 problems to be found");
    }
}
