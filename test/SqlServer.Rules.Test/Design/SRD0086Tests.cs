using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0086Tests : TestModel
{
    public SRD0086Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void AnsiPaddingOffDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/AnsiPaddingOffTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRD0086"));

        RunTest();
    }
}
