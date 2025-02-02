using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestAW : TestModel
{
    public TestAW()
    {
        foreach (var fileName in Directory.GetFiles("../../../../AW/Tables", "*.sql"))
        {
            TestFiles.Add(fileName);
        }

        ExpectedProblems.Add(new TestProblem(8, 7, "Smells.SML006"));
    }

    [TestMethod]
    [Ignore("Will add proper assert later")]
    public void TestAdventureworks()
    {
        RunTest();
    }
}

