﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testWithNoLockIndexhint : TestModel
{
    public testWithNoLockIndexhint()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLockIndexhint.sql");

        ExpectedProblems.Add(new TestProblem(4, 42, "Smells.SML003"));
        ExpectedProblems.Add(new TestProblem(4, 49, "Smells.SML045"));
    }

    [TestMethod]
    public void WithNoLockIndexhint()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
