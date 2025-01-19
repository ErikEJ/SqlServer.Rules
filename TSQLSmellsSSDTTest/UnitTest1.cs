using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

#pragma warning disable IDE1006 // Naming Styles
[TestClass]
public class testConvertDate : TestModel
{
    public testConvertDate()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ConvertDate.sql");

        ExpectedProblems.Add(new TestProblem(8, 7, "Smells.SML006"));
    }

    [TestMethod]
    public void ConvertDate()
    {
        RunTest();
    }
}

[TestClass]
public class testsqlInjection : TestModel
{
    public testsqlInjection()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/Inject.sql");

        ExpectedProblems.Add(new TestProblem(14, 10, "Smells.SML043"));
        ExpectedProblems.Add(new TestProblem(23, 10, "Smells.SML043"));
        ExpectedProblems.Add(new TestProblem(52, 10, "Smells.SML043"));
        ExpectedProblems.Add(new TestProblem(88, 10, "Smells.SML043"));
        ExpectedProblems.Add(new TestProblem(5, 7, "Smells.SML043"));
    }

    [TestMethod]
    public void SQLInjection()
    {
        RunTest();
    }
}

[TestClass]
public class testCreateViewOrderBy : TestModel
{
    public testCreateViewOrderBy()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/CreateViewOrderBy.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML028"));
    }

    [TestMethod]
    public void CreateViewOrderBy()
    {
        RunTest();
    }
}

[TestClass]
public class testConvertDateMultipleCond : TestModel
{
    public testConvertDateMultipleCond()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ConvertDateMultiCond.sql");

        ExpectedProblems.Add(new TestProblem(7, 7, "Smells.SML006"));
        ExpectedProblems.Add(new TestProblem(8, 5, "Smells.SML006"));
    }

    [TestMethod]
    public void ConvertDateMultipleCond()
    {
        RunTest();
    }
}

[TestClass]
public class testDisabledForeignKeyConstraint : TestModel
{

    public testDisabledForeignKeyConstraint()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/DisabledForeignKey.sql");

        ExpectedProblems.Add(new TestProblem(7, 7, "Smells.SML006"));
        ExpectedProblems.Add(new TestProblem(8, 5, "Smells.SML006"));
    }

    [TestMethod, Ignore("Not working")]
    public void DisabledForeignKeyConstraint()
    {
        RunTest();
    }

}

[TestClass]
public class testConvertInt : TestModel
{
    public testConvertInt()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ConvertInt.sql");

        ExpectedProblems.Add(new TestProblem(7, 7, "Smells.SML006"));
    }

    [TestMethod]
    public void ConvertInt()
    {
        RunTest();
    }
}

[TestClass]
public class testConvertInt2 : TestModel
{
    public testConvertInt2()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ConvertInt2.sql");

        ExpectedProblems.Add(new TestProblem(7, 14, "Smells.SML006"));
    }

    [TestMethod]
    public void ConvertInt2()
    {
        RunTest();
    }
}

[TestClass]
public class testCreateProcedureNoSchema : TestModel
{
    public testCreateProcedureNoSchema()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/CreateProcedureNoSchema.sql");

        ExpectedProblems.Add(new TestProblem(2, 18, "Smells.SML024"));
    }

    [TestMethod]
    public void CreateProcedureNoSchema()
    {
        RunTest();
    }
}

[TestClass]
public class testCreateTableNoSchema : TestModel
{
    public testCreateTableNoSchema()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/CreateTableNoSchema.sql");

        ExpectedProblems.Add(new TestProblem(1, 1, "Smells.SML027"));
    }

    [TestMethod]
    public void CreateTableNoSchema()
    {
        RunTest();
    }
}

[TestClass]
public class testDeclareCursor : TestModel
{
    public testDeclareCursor()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/DeclareCursor.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML029"));
    }

    [TestMethod]
    public void DeclareCursor()
    {
        RunTest();
    }
}

[TestClass]
public class testDerived : TestModel
{
    public testDerived()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/Derived.sql");

        ExpectedProblems.Add(new TestProblem(7, 24, "Smells.SML035"));
    }

    [TestMethod]
    public void Derived()
    {
        RunTest();
    }
}

[TestClass]
public class testExec1PartName : TestModel
{
    public testExec1PartName()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/Exec1PartName.sql");

        ExpectedProblems.Add(new TestProblem(5, 6, "Smells.SML021"));
    }

    [TestMethod]
    public void Exec1PartName()
    {
        RunTest();
    }
}

[TestClass]
public class testExecSQL : TestModel
{
    public testExecSQL()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ExecSQL.sql");

        ExpectedProblems.Add(new TestProblem(6, 1, "Smells.SML012"));
    }

    [TestMethod]
    public void ExecSQL()
    {
        RunTest();
    }
}

[TestClass]
public class testExplicitRangeWindow : TestModel
{
    public testExplicitRangeWindow()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ExplicitRangeWindow.sql");

        ExpectedProblems.Add(new TestProblem(7, 19, "Smells.SML025"));
    }

    [TestMethod]
    public void ExplicitRangeWindow()
    {
        RunTest();
    }
}

[TestClass]
public class testExists : TestModel
{
    public testExists()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/Exists.sql");

        // this._ExpectedProblems.Add(new TestProblem(7, 19, "Smells.SML025"));
    }

    [TestMethod]
    public void Exists()
    {
        RunTest();
    }
}

[TestClass]
public class testForceScan : TestModel
{
    public testForceScan()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ForceScan.sql");

        ExpectedProblems.Add(new TestProblem(6, 30, "Smells.SML044"));
    }

    [TestMethod]
    public void ForceScan()
    {
        RunTest();
    }
}

[TestClass]
public class testImplicitRangeWindow : TestModel
{
    public testImplicitRangeWindow()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ImplicitRangeWindow.sql");

        ExpectedProblems.Add(new TestProblem(5, 32, "Smells.SML026"));
    }

    [TestMethod]
    public void ImplicitRangeWindow()
    {
        RunTest();
    }
}

[TestClass]
public class testInsertMissingColumnSpecifiers : TestModel
{
    public testInsertMissingColumnSpecifiers()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/InsertMissingColumnSpecifiers.sql");

        ExpectedProblems.Add(new TestProblem(5, 5, "Smells.SML012"));
    }

    [TestMethod]
    public void InsertMissingColumnSpecifiers()
    {
        RunTest();
    }
}

[TestClass]
public class testInsertSelectStar : TestModel
{
    public testInsertSelectStar()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/InsertSelectStar.sql");

        ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void InsertSelectStar()
    {
        RunTest();
    }
}

[TestClass]
public class testLagFunction : TestModel
{
    public testLagFunction()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/LAGFunction.sql");

        // this._ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void LagFunction()
    {
        RunTest();
    }
}

[TestClass]
public class testMultiCteTest : TestModel
{
    public testMultiCteTest()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/MultiCteTest.sql");

        ExpectedProblems.Add(new TestProblem(8, 10, "Smells.SML005"));
    }

    [TestMethod]
    public void MultiCteTest()
    {
        RunTest();
    }
}

[TestClass]
public class testOrderByOrdinal : TestModel
{
    public testOrderByOrdinal()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/orderbyordinal.sql");

        ExpectedProblems.Add(new TestProblem(6, 34, "Smells.SML007"));
        ExpectedProblems.Add(new TestProblem(6, 36, "Smells.SML007"));
        ExpectedProblems.Add(new TestProblem(6, 38, "Smells.SML007"));
    }

    [TestMethod]
    public void OrderByOrdinal()
    {
        RunTest();
    }
}

[TestClass]
public class testRangeWindow : TestModel
{
    public testRangeWindow()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/RangeWindow.sql");

        ExpectedProblems.Add(new TestProblem(8, 19, "Smells.SML025"));
    }

    [TestMethod]
    public void RangeWindow()
    {
        RunTest();
    }
}

[TestClass]
public class testSelectFromTableVar : TestModel
{
    public testSelectFromTableVar()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SelectFromTableVar.sql");

        ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(4, 1, "Smells.SML033"));
    }

    [TestMethod]
    public void SelectFromTableVar()
    {
        RunTest();
    }
}

[TestClass]
public class testSelectStarFromViewInProc : TestModel
{
    public testSelectStarFromViewInProc()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SelectStarFromViewInProc.sql");

        ExpectedProblems.Add(new TestProblem(4, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectFromTableVar()
    {
        RunTest();
    }
}

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

[TestClass]
public class testSelectTopNoParen : TestModel
{
    public testSelectTopNoParen()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SelectTopNoParen.sql");

        ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML034"));
    }

    [TestMethod]
    public void SelectTopNoParen()
    {
        RunTest();
    }
}

[TestClass]
public class testSetNoCountON : TestModel
{
    public testSetNoCountON()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SetNoCountON.sql");

        // this._ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML034"));
    }

    [TestMethod]
    public void SetNoCountON()
    {
        RunTest();
    }
}

[TestClass]
public class testSets : TestModel
{
    public testSets()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SETs.sql");

        ExpectedProblems.Add(new TestProblem(10, 1, "Smells.SML013"));
        ExpectedProblems.Add(new TestProblem(4, 1, "Smells.SML014"));
        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML015"));
        ExpectedProblems.Add(new TestProblem(6, 1, "Smells.SML016"));
        ExpectedProblems.Add(new TestProblem(7, 1, "Smells.SML017"));
        ExpectedProblems.Add(new TestProblem(8, 1, "Smells.SML018"));
        ExpectedProblems.Add(new TestProblem(9, 1, "Smells.SML019"));
        ExpectedProblems.Add(new TestProblem(2, 18, "Smells.SML030"));
    }

    [TestMethod]
    public void Sets()
    {
        RunTest();
    }
}

[TestClass]
public class testSets2 : TestModel
{
    public testSets2()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SETs2.sql");

        ExpectedProblems.Add(new TestProblem(5, 16, "Smells.SML008"));
        ExpectedProblems.Add(new TestProblem(6, 15, "Smells.SML009"));
        ExpectedProblems.Add(new TestProblem(7, 1, "Smells.SML020"));
        ExpectedProblems.Add(new TestProblem(8, 1, "Smells.SML022"));
    }

    [TestMethod]
    public void Sets2()
    {
        RunTest();
    }
}

[TestClass]
public class testSetTransactionIsolationLevel : TestModel
{
    public testSetTransactionIsolationLevel()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SetTransactionIsolationLevel.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML010"));
    }

    [TestMethod]
    public void SetTransactionIsolationLevel()
    {
        RunTest();
    }
}

[TestClass]
public class testSingleCharAlias : TestModel
{
    public testSingleCharAlias()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SingleCharAlias.sql");

        ExpectedProblems.Add(new TestProblem(6, 8, "Smells.SML011"));
    }

    [TestMethod]
    public void SingleCharAlias()
    {
        RunTest();
    }
}

[TestClass]
public class testTableHints : TestModel
{
    public testTableHints()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TableHints.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML004"));
    }

    [TestMethod]
    public void TableHints()
    {
        RunTest();
    }
}

[TestClass]
public class testCrossServerJoin : TestModel
{
    public testCrossServerJoin()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestCrossServerJoin.sql");

        ExpectedProblems.Add(new TestProblem(5, 18, "Smells.SML001"));
    }

    [TestMethod]
    public void CrossServerJoin()
    {
        RunTest();
    }
}

[TestClass]
public class testOnePartNamedSelect : TestModel
{
    public testOnePartNamedSelect()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestOnePartNamedSelect.sql");

        ExpectedProblems.Add(new TestProblem(6, 19, "Smells.SML002"));
    }

    [TestMethod]
    public void OnePartNamedSelect()
    {
        RunTest();
    }
}

[TestClass]
public class testSelectStarBeginEndBlock : TestModel
{
    public testSelectStarBeginEndBlock()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarBeginEndBlock.sql");

        ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarBeginEndBlock()
    {
        RunTest();
    }
}

[TestClass]
public class testSelectStarInCteTest : TestModel
{
    public testSelectStarInCteTest()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInCteTest.sql");

        ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarInCteTest()
    {
        RunTest();
    }
}

[TestClass]
public class testSelectStarIninlineTVF : TestModel
{
    public testSelectStarIninlineTVF()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarIninlineTVF.sql");

        ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarIninlineTVF()
    {
        RunTest();
    }
}

[TestClass]
public class testSelectStarInMultiStatementTVF : TestModel
{
    public testSelectStarInMultiStatementTVF()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInMultiStatementTVF.sql");

        ExpectedProblems.Add(new TestProblem(12, 10, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(8, 10, "Smells.SML033"));
    }

    [TestMethod]
    public void SelectStarInMultiStatementTVF()
    {
        RunTest();
    }
}

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

[TestClass]
public class testSelectStarInWhileLoop : TestModel
{
    public testSelectStarInWhileLoop()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInWhileLoop.sql");

        ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarInWhileLoop()
    {
        RunTest();
    }
}

[TestClass]
public class testWithExists : TestModel
{
    public testWithExists()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithExists.sql");

        ExpectedProblems.Add(new TestProblem(5, 18, "Smells.SML005"));
    }

    [TestMethod]
    public void WithExists()
    {
        RunTest();
    }
}

[TestClass]
public class testWithExistsAndNestedSelectStar : TestModel
{
    public testWithExistsAndNestedSelectStar()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithExistsAndNestedSelectStar.sql");

        ExpectedProblems.Add(new TestProblem(4, 18, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void WithExistsAndNestedSelectStar()
    {
        RunTest();
    }
}

[TestClass]
public class testWithExistsAndNestedSelectStarInlineIF : TestModel
{
    public testWithExistsAndNestedSelectStarInlineIF()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithExistsAndNestedSelectStarInlineIF.sql");

        ExpectedProblems.Add(new TestProblem(4, 18, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(4, 51, "Smells.SML005"));
    }

    [TestMethod]
    public void WithExistsAndNestedSelectStarInlineIF()
    {
        RunTest();
    }
}

[TestClass]
public class testWithNoLock : TestModel
{
    public testWithNoLock()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLock.sql");

        ExpectedProblems.Add(new TestProblem(4, 42, "Smells.SML003"));
    }

    [TestMethod]
    public void WithNoLock()
    {
        RunTest();
    }
}

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

[TestClass]
public class testWithNoLockInWhiteList : TestModel
{
    public testWithNoLockInWhiteList()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLockInWhiteList.sql");

        // this._ExpectedProblems.Add(new TestProblem(4, 42, "Smells.SML003"));
    }

    [TestMethod]
    public void WithNoLockInWhiteList()
    {
        RunTest();
    }
}

[TestClass]
public class testWithNoLockNoWith : TestModel
{
    public testWithNoLockNoWith()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLockNoWith.sql");

        ExpectedProblems.Add(new TestProblem(4, 38, "Smells.SML003"));
    }

    [TestMethod]
    public void WithNoLockNoWith()
    {
        RunTest();
    }
}

[TestClass]
public class testUnion : TestModel
{
    public testUnion()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/UnionTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 8, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void Union()
    {
        RunTest();
    }
}

[TestClass]
public class testUnnamedPrimaryKey : TestModel
{
    public testUnnamedPrimaryKey()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/UnnamedPK.sql");

        // this._ExpectedProblems.Add(new TestProblem(5, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void UnnamedPrimaryKey()
    {
        RunTest();
    }
}

[TestClass]
public class testWhiteListTest : TestModel
{
    public testWhiteListTest()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/WhiteListTest.sql");

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

[TestClass]
public class testSingleLineComment : TestModel
{
    public testSingleLineComment()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SingleLineComment.sql");

        // this._ExpectedProblems.Add(new TestProblem(5, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void SingleLineComment()
    {
        RunTest();
    }
}

[TestClass]
public class testTempTableWithNamedPK : TestModel
{
    public testTempTableWithNamedPK()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedPK.sql");

        ExpectedProblems.Add(new TestProblem(14, 3, "Smells.SML038"));

        // this._ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void TempTableWithNamedPK()
    {
        RunTest();
    }
}

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

[TestClass]
public class testTempTableWithNamedCheckConstraint : TestModel
{
    public testTempTableWithNamedCheckConstraint()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedCheckConstraint.sql");

        ExpectedProblems.Add(new TestProblem(14, 16, "Smells.SML040"));
    }

    [TestMethod]
    public void TempTableWithNamedCheckConstraint()
    {
        RunTest();
    }
}

[TestClass]
public class testEqualsNull : TestModel
{
    public testEqualsNull()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/EqualsNull.sql");
        ExpectedProblems.Add(new TestProblem(13, 39, "Smells.SML046"));
    }

    [TestMethod]
    public void EqualsNull()
    {
        RunTest();
    }
}

[TestClass]
public class testDeprecatedType : TestModel
{
    public testDeprecatedType()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/DeprecatedTypes.sql");
        ExpectedProblems.Add(new TestProblem(4, 16, "Smells.SML047"));
    }

    [TestMethod]
    public void DeprecatedTypes()
    {
        RunTest();
    }
}

[TestClass]
public class testDeprecatedTypeSP : TestModel
{
    public testDeprecatedTypeSP()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/DeprecatedTypesSP.sql");
        ExpectedProblems.Add(new TestProblem(4, 14, "Smells.SML047"));
        ExpectedProblems.Add(new TestProblem(5, 14, "Smells.SML047"));
    }

    [TestMethod]
    public void DeprecatedTypesSP()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
