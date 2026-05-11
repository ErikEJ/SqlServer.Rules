using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0084Tests : TestModel
{
    public SRD0084Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void ConcatNullYieldsNullOffDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/ConcatNullYieldsNullOffTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRD0084"));

        RunTest();
    }
}
