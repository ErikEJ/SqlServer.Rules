using System;
using System.Linq;
using ErikEJ.DacFX.TSQLAnalyzer.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Analyzer")]
public class BatchWrapperTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void WrapReturnsInputForNullOrWhitespace(string sql)
    {
        Assert.AreEqual(sql, BatchWrapper.Wrap(sql));
    }

    [TestMethod]
    public void WrapLeavesProcedureDefinitionUnchanged()
    {
        var sql = "CREATE PROCEDURE dbo.TestProc AS SELECT 1;";

        Assert.AreEqual(sql, BatchWrapper.Wrap(sql));
    }

    [TestMethod]
    public void WrapLeavesTableDefinitionUnchanged()
    {
        var sql = "CREATE TABLE dbo.Foo (Id INT NOT NULL);";

        Assert.AreEqual(sql, BatchWrapper.Wrap(sql));
    }

    [TestMethod]
    public void WrapEnclosesDmlBatchInSyntheticProcedure()
    {
        var sql = "SELECT * FROM sys.objects;";

        var wrapped = BatchWrapper.Wrap(sql);

        Assert.AreNotEqual(sql, wrapped);
        StringAssert.Contains(wrapped, $"CREATE PROCEDURE [dbo].[{BatchWrapper.SyntheticObjectPrefix}1] AS BEGIN ", StringComparison.Ordinal);
        StringAssert.Contains(wrapped, "SELECT * FROM sys.objects;", StringComparison.Ordinal);
        StringAssert.EndsWith(wrapped, " END;", StringComparison.Ordinal);
    }

    [TestMethod]
    public void WrapDoesNotAddLinesSoOriginalLineNumbersArePreserved()
    {
        var sql = "-- a comment\nSELECT *\nFROM sys.objects;\n";

        var wrapped = BatchWrapper.Wrap(sql);

        Assert.AreNotEqual(sql, wrapped);
        Assert.AreEqual(
            sql.Count(c => c == '\n'),
            wrapped.Count(c => c == '\n'),
            "Wrapping must be inline so reported line numbers still match the original source.");
    }

    [TestMethod]
    public void WrapEnclosesEachAdhocBatchIndependently()
    {
        var sql = "SELECT * FROM sys.objects;\nGO\nUPDATE dbo.Foo SET Id = 1;\nGO\n";

        var wrapped = BatchWrapper.Wrap(sql);

        StringAssert.Contains(wrapped, $"[{BatchWrapper.SyntheticObjectPrefix}1]", StringComparison.Ordinal);
        StringAssert.Contains(wrapped, $"[{BatchWrapper.SyntheticObjectPrefix}2]", StringComparison.Ordinal);
    }

    [TestMethod]
    public void WrapLeavesObjectDefinitionUntouchedAmongAdhocBatches()
    {
        var sql = "CREATE TABLE dbo.Foo (Id INT NOT NULL);\nGO\nSELECT * FROM dbo.Foo;\nGO\n";

        var wrapped = BatchWrapper.Wrap(sql);

        // The object definition keeps exactly one CREATE TABLE and is not nested in a procedure.
        StringAssert.Contains(wrapped, "CREATE TABLE dbo.Foo (Id INT NOT NULL);", StringComparison.Ordinal);

        // Only the ad-hoc DML batch is wrapped, so there is a single synthetic procedure.
        StringAssert.Contains(wrapped, $"[{BatchWrapper.SyntheticObjectPrefix}1]", StringComparison.Ordinal);
        Assert.IsFalse(
            wrapped.Contains($"[{BatchWrapper.SyntheticObjectPrefix}2]", StringComparison.Ordinal),
            "The object-definition batch must not be wrapped.");
    }

    [TestMethod]
    public void WrapLeavesBatchOnlyStatementUntouched()
    {
        // USE is a batch-only statement that is illegal inside a procedure body, so it is not wrapped.
        var sql = "USE master;";

        Assert.AreEqual(sql, BatchWrapper.Wrap(sql));
    }

    [TestMethod]
    public void WrapReturnsUnparseableInputVerbatim()
    {
        var sql = "this is not valid t-sql @@@";

        Assert.AreEqual(sql, BatchWrapper.Wrap(sql));
    }
}
