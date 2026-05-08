using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0079Tests : TestModel
{
    public SRD0079Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void SingleCharacterVariableDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SingleCharVariableTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 13, "SqlServer.Rules.SRD0079"));

        RunTest();
    }
}
