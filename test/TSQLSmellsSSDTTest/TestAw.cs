using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestAW : TestModel
{
    [TestMethod]
    public void TestAdventureworks()
    {
        foreach (var fileName in Directory.GetFiles("../../../../../sqlprojects/AW/Tables", "*.sql"))
        {
            TestFiles.Add(fileName);
        }

        ExpectedProblems.Add(new TestProblem(18, 20, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(27, 20, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(42, 20, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(57, 20, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(20, 4, "Smells.SML012"));
        ExpectedProblems.Add(new TestProblem(31, 4, "Smells.SML012"));
        ExpectedProblems.Add(new TestProblem(46, 4, "Smells.SML012"));
        ExpectedProblems.Add(new TestProblem(61, 4, "Smells.SML012"));

        RunTest();
    }
}

