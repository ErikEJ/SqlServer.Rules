using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest
{
    public class TestProblem
    {
        public int StartColumn;
        public int StartLine;
        public string RuleId;

        public TestProblem(int StartLine, int StartColumn, string RuleId)
        {
            this.StartColumn = StartColumn;
            this.StartLine = StartLine;
            this.RuleId = RuleId;
        }

        public override bool Equals(Object obj)
        {
            TestProblem prb = obj as TestProblem;
            if (prb.RuleId.Equals(RuleId, StringComparison.OrdinalIgnoreCase) &&
                prb.StartColumn == StartColumn &&
                prb.StartLine == StartLine)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return string.Format("{0}:{1}:{2}", RuleId, StartColumn, StartLine).GetHashCode();
        }
    }

    public class TestModel
    {
        public List<TestProblem> _ExpectedProblems = new List<TestProblem>();
        public List<TestProblem> _FoundProblems = new List<TestProblem>();
        public List<String> _TestFiles = new List<String>();

        private TSqlModel _Model;

        public void BuildModel()
        {
            _Model = new TSqlModel(SqlServerVersion.Sql110, null);
            AddFilesToModel();
        }

        public void AddFilesToModel()
        {
            foreach (string FileName in _TestFiles)
            {
                String FileContent = string.Empty;
                using (var reader = new StreamReader(FileName))
                {
                    FileContent += reader.ReadToEnd();
                }

                _Model.AddObjects(FileContent);
            }
        }

        public void SerializeResultOutput(CodeAnalysisResult result)
        {
            foreach (SqlRuleProblem Problem in result.Problems)
            {
                // Only concern ourselves with our problems
                if (Problem.RuleId.StartsWith("Smells."))
                {
                    TestProblem TestProblem = new TestProblem(Problem.StartLine, Problem.StartColumn, Problem.RuleId);
                    _FoundProblems.Add(TestProblem);
                }
            }
        }

        public void RunSCARules()
        {
            CodeAnalysisService service = new CodeAnalysisServiceFactory().CreateAnalysisService(_Model.Version);
            CodeAnalysisResult result = service.Analyze(_Model);
            SerializeResultOutput(result);

            CollectionAssert.AreEquivalent(_FoundProblems, _ExpectedProblems);
        }

        public void RunTest()
        {
            BuildModel();
            RunSCARules();
        }
    }

    [TestClass]
    public class testConvertDate : TestModel
    {
        public testConvertDate()
        {
            _TestFiles.Add("../../../../TSQLSmellsTest/ConvertDate.sql");

            _ExpectedProblems.Add(new TestProblem(8, 7, "Smells.SML006"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/Inject.sql");

            _ExpectedProblems.Add(new TestProblem(14, 10, "Smells.SML043"));
            _ExpectedProblems.Add(new TestProblem(23, 10, "Smells.SML043"));
            _ExpectedProblems.Add(new TestProblem(52, 10, "Smells.SML043"));
            _ExpectedProblems.Add(new TestProblem(88, 10, "Smells.SML043"));
            _ExpectedProblems.Add(new TestProblem(5, 7, "Smells.SML043"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/CreateViewOrderBy.sql");

            _ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML028"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/ConvertDateMultiCond.sql");

            _ExpectedProblems.Add(new TestProblem(7, 7, "Smells.SML006"));
            _ExpectedProblems.Add(new TestProblem(8, 5, "Smells.SML006"));
        }

        [TestMethod]
        public void ConvertDateMultipleCond()
        {
            RunTest();
        }
    }

    /*
    [TestClass]
    public class testDisabledForeignKeyConstraint : TestModel
    {

        public testDisabledForeignKeyConstraint()
        {
            this._TestFiles.Add("../../../../TSQLSmellsTest/DisabledForeignKey.sql");

            this._ExpectedProblems.Add(new TestProblem(7, 7, "Smells.SML006"));
            this._ExpectedProblems.Add(new TestProblem(8, 5, "Smells.SML006"));
        }

        [TestMethod]
        public void DisabledForeignKeyConstraint()
        {

            RunTest();
        }

    }
    */
    [TestClass]
    public class testConvertInt : TestModel
    {
        public testConvertInt()
        {
            _TestFiles.Add("../../../../TSQLSmellsTest/ConvertInt.sql");

            _ExpectedProblems.Add(new TestProblem(7, 7, "Smells.SML006"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/ConvertInt2.sql");

            _ExpectedProblems.Add(new TestProblem(7, 14, "Smells.SML006"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/CreateProcedureNoSchema.sql");

            _ExpectedProblems.Add(new TestProblem(2, 18, "Smells.SML024"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/CreateTableNoSchema.sql");

            _ExpectedProblems.Add(new TestProblem(1, 1, "Smells.SML027"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/DeclareCursor.sql");

            _ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML029"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/Derived.sql");

            _ExpectedProblems.Add(new TestProblem(7, 24, "Smells.SML035"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/Exec1PartName.sql");

            _ExpectedProblems.Add(new TestProblem(5, 6, "Smells.SML021"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/ExecSQL.sql");

            _ExpectedProblems.Add(new TestProblem(6, 1, "Smells.SML012"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/ExplicitRangeWindow.sql");

            _ExpectedProblems.Add(new TestProblem(7, 19, "Smells.SML025"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/Exists.sql");

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
            _TestFiles.Add("../../../../TSQLSmellsTest/ForceScan.sql");

            _ExpectedProblems.Add(new TestProblem(6, 30, "Smells.SML044"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/ImplicitRangeWindow.sql");

            _ExpectedProblems.Add(new TestProblem(5, 32, "Smells.SML026"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/InsertMissingColumnSpecifiers.sql");

            _ExpectedProblems.Add(new TestProblem(5, 5, "Smells.SML012"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/InsertSelectStar.sql");

            _ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/LAGFunction.sql");

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
            _TestFiles.Add("../../../../TSQLSmellsTest/MultiCteTest.sql");

            _ExpectedProblems.Add(new TestProblem(8, 10, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/orderbyordinal.sql");

            _ExpectedProblems.Add(new TestProblem(6, 34, "Smells.SML007"));
            _ExpectedProblems.Add(new TestProblem(6, 36, "Smells.SML007"));
            _ExpectedProblems.Add(new TestProblem(6, 38, "Smells.SML007"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/RangeWindow.sql");

            _ExpectedProblems.Add(new TestProblem(8, 19, "Smells.SML025"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/SelectFromTableVar.sql");

            _ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
            _ExpectedProblems.Add(new TestProblem(4, 1, "Smells.SML033"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/SelectStarFromViewInProc.sql");

            _ExpectedProblems.Add(new TestProblem(4, 8, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/SelectStarOutOfCteTest.sql");

            _ExpectedProblems.Add(new TestProblem(8, 8, "Smells.SML005"));
            _ExpectedProblems.Add(new TestProblem(10, 8, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/SelectTopNoParen.sql");

            _ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML034"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/SetNoCountON.sql");

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
            _TestFiles.Add("../../../../TSQLSmellsTest/SETs.sql");

            _ExpectedProblems.Add(new TestProblem(10, 1, "Smells.SML013"));
            _ExpectedProblems.Add(new TestProblem(4, 1, "Smells.SML014"));
            _ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML015"));
            _ExpectedProblems.Add(new TestProblem(6, 1, "Smells.SML016"));
            _ExpectedProblems.Add(new TestProblem(7, 1, "Smells.SML017"));
            _ExpectedProblems.Add(new TestProblem(8, 1, "Smells.SML018"));
            _ExpectedProblems.Add(new TestProblem(9, 1, "Smells.SML019"));
            _ExpectedProblems.Add(new TestProblem(2, 18, "Smells.SML030"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/SETs2.sql");

            _ExpectedProblems.Add(new TestProblem(5, 16, "Smells.SML008"));
            _ExpectedProblems.Add(new TestProblem(6, 15, "Smells.SML009"));
            _ExpectedProblems.Add(new TestProblem(7, 1, "Smells.SML020"));
            _ExpectedProblems.Add(new TestProblem(8, 1, "Smells.SML022"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/SetTransactionIsolationLevel.sql");

            _ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML010"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/SingleCharAlias.sql");

            _ExpectedProblems.Add(new TestProblem(6, 8, "Smells.SML011"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TableHints.sql");

            _ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML004"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestCrossServerJoin.sql");

            _ExpectedProblems.Add(new TestProblem(5, 18, "Smells.SML001"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestOnePartNamedSelect.sql");

            _ExpectedProblems.Add(new TestProblem(6, 19, "Smells.SML002"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarBeginEndBlock.sql");

            _ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInCteTest.sql");

            _ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarIninlineTVF.sql");

            _ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInMultiStatementTVF.sql");

            _ExpectedProblems.Add(new TestProblem(12, 10, "Smells.SML005"));
            _ExpectedProblems.Add(new TestProblem(8, 10, "Smells.SML033"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInScalarUDF.sql");

            _ExpectedProblems.Add(new TestProblem(9, 10, "Smells.SML005"));
            _ExpectedProblems.Add(new TestProblem(5, 10, "Smells.SML033"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInWhileLoop.sql");

            _ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestWithExists.sql");

            _ExpectedProblems.Add(new TestProblem(5, 18, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestWithExistsAndNestedSelectStar.sql");

            _ExpectedProblems.Add(new TestProblem(4, 18, "Smells.SML005"));
            _ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestWithExistsAndNestedSelectStarInlineIF.sql");

            _ExpectedProblems.Add(new TestProblem(4, 18, "Smells.SML005"));
            _ExpectedProblems.Add(new TestProblem(4, 51, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLock.sql");

            _ExpectedProblems.Add(new TestProblem(4, 42, "Smells.SML003"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLockIndexhint.sql");

            _ExpectedProblems.Add(new TestProblem(4, 42, "Smells.SML003"));
            _ExpectedProblems.Add(new TestProblem(4, 49, "Smells.SML045"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLockInWhiteList.sql");

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
            _TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLockNoWith.sql");

            _ExpectedProblems.Add(new TestProblem(4, 38, "Smells.SML003"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/UnionTest.sql");

            _ExpectedProblems.Add(new TestProblem(5, 8, "Smells.SML005"));
            _ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
            _ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/UnnamedPK.sql");

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
            _TestFiles.Add("../../../../TSQLSmellsTest/WhiteListTest.sql");

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
            _TestFiles.Add("../../../../TSQLSmellsTest/SingleLineComment.sql");

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
            _TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedPK.sql");

            _ExpectedProblems.Add(new TestProblem(14, 3, "Smells.SML038"));

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
            _TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedDefConstraint.sql");

            _ExpectedProblems.Add(new TestProblem(14, 3, "Smells.SML039"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedFK.sql");

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
            _TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedCheckConstraint.sql");

            _ExpectedProblems.Add(new TestProblem(14, 16, "Smells.SML040"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/EqualsNull.sql");
            _ExpectedProblems.Add(new TestProblem(13, 39, "Smells.SML046"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/DeprecatedTypes.sql");
            _ExpectedProblems.Add(new TestProblem(4, 16, "Smells.SML047"));
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
            _TestFiles.Add("../../../../TSQLSmellsTest/DeprecatedTypesSP.sql");
            _ExpectedProblems.Add(new TestProblem(4, 14, "Smells.SML047"));
            _ExpectedProblems.Add(new TestProblem(5, 14, "Smells.SML047"));
        }

        [TestMethod]
        public void DeprecatedTypesSP()
        {
            RunTest();
        }
    }
}
