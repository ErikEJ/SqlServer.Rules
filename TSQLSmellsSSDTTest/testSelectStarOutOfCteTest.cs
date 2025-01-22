﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSelectStarOutOfCteTest : TestModel
{
    public testSelectStarOutOfCteTest()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SelectStarOutOfCteTest.sql");

        ExpectedProblems.Add(new TestProblem(8, 8, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(10, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarOutOfCteTest()
    {
        RunTest();
    }
}

