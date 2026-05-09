using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0094Tests : TestModel
{
    public SRD0094Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void NamedFKOnTempTableDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TempTableWithNamedFKConstraint.sql");

        ExpectedProblems.Add(new TestProblem(17, 18, "SqlServer.Rules.SRD0094"));

        RunTest();
    }

    [TestMethod]
    public void PermanentTableWithHashInNameIsIgnored()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/PermanentTableWithHashInNameAndNamedFK.sql");

        RunTest();
    }

    [TestMethod]
    public void TableLevelNamedFKOnTempTableDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TempTableWithTableLevelNamedFKConstraint.sql");

        ExpectedProblems.Add(new TestProblem(13, 5, "SqlServer.Rules.SRD0094"));

        RunTest();
    }
}
