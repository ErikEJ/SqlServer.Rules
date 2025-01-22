﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSelectStarInScalarUDF : TestModel
{
    public testSelectStarInScalarUDF()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInScalarUDF.sql");

        ExpectedProblems.Add(new TestProblem(9, 10, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(5, 10, "Smells.SML033"));
    }

    [TestMethod]
    public void SelectStarInScalarUDF()
    {
        RunTest();
    }
}

