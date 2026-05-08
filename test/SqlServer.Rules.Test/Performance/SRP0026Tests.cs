using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Performance;

[TestClass]
public class SRP0026Tests : TestModel
{
    public SRP0026Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void CrossServerJoinDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CrossServerJoinSRP0026Test.sql");

        ExpectedProblems.Add(new TestProblem(6, 6, "SqlServer.Rules.SRP0026"));

        RunTest();
    }
}
