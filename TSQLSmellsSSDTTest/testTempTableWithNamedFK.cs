﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testTempTableWithNamedFK : TestModel
{
    public testTempTableWithNamedFK()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedFK.sql");

        // this._ExpectedProblems.Add(new TestProblem(14, 3, "Smells.SML040"));
    }

    [TestMethod]
    public void TempTableWithNamedFK()
    {
        RunTest();
    }
}

