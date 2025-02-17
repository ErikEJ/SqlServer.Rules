using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestWhiteListTest : TestModel
{
    public TestWhiteListTest()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/WhiteListTest.sql");

        // this._ExpectedProblems.Add(new TestProblem(5, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void WhiteList()
    {
        RunTest();
    }
}

