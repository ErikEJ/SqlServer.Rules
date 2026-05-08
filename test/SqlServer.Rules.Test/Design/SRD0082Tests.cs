using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0082Tests : TestModel
{
    public SRD0082Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void DateFormatDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/DateFormatTest.sql");

        ExpectedProblems.Add(new TestProblem(4, 1, "SqlServer.Rules.SRD0082"));

        RunTest();
    }
}
