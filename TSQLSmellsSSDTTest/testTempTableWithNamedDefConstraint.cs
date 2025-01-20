﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testTempTableWithNamedDefConstraint : TestModel
{
    public testTempTableWithNamedDefConstraint()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedDefConstraint.sql");

        ExpectedProblems.Add(new TestProblem(14, 3, "Smells.SML039"));
    }

    [TestMethod]
    public void TempTableWithNamedDefConstraint()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
