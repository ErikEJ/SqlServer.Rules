using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Performance;

[TestClass]
public class SRP0027Tests : TestModel
{
    public SRP0027Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void ExplicitColumnConversionDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/ExplicitColumnConversion.sql");

        ExpectedProblems.Add(new TestProblem(7, 7, "SqlServer.Rules.SRP0027"));

        RunTest();
    }
}
