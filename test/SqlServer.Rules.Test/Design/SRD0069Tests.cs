using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0069Tests : TestModel
{
    public SRD0069Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void FutureKeywords()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/FutureKeywords.sql");

        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0069"));

        RunTest();
    }
}
