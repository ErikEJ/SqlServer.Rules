using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0074Tests : TestModel
{
    public SRD0074Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void WeakHashingDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/WeakHashingTest.sql");

        ExpectedProblems.Add(new TestProblem(6, 17, "SqlServer.Rules.SRD0074"));
        ExpectedProblems.Add(new TestProblem(9, 18, "SqlServer.Rules.SRD0074"));

        RunTest();
    }

    [TestMethod]
    public void StrongHashingClean()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/WeakHashingCleanTest.sql");

        RunTest();
    }
}
