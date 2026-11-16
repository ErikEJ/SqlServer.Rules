using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Performance;
using SqlServer.Rules.Tests.Utils;

namespace SqlServer.Rules.Tests.Performance;

[TestClass]
[TestCategory("Performance")]
public class SRP0023Tests : TestCasesBase
{
    private const string TableScript = """
                                     CREATE TABLE dbo.TestTable
                                     (
                                         Id INT NOT NULL,
                                         NullableId INT NULL
                                     );
                                     """;

    [TestMethod]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) > 0", DisplayName = "count > 0")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) >= 1", DisplayName = "count >= 1")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) <> 0", DisplayName = "count <> 0")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) != 0", DisplayName = "count != 0")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) = 0", DisplayName = "count = 0")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) < 1", DisplayName = "count < 1")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) <= 0", DisplayName = "count <= 0")]
    [DataRow("0 < (SELECT COUNT(*) FROM dbo.TestTable)", DisplayName = "0 < count")]
    [DataRow("1 <= (SELECT COUNT(*) FROM dbo.TestTable)", DisplayName = "1 <= count")]
    [DataRow("0 = (SELECT COUNT(*) FROM dbo.TestTable)", DisplayName = "0 = count")]
    public void ExistenceComparisonDetected(string predicate)
    {
        var procedureScript = $"""
                              CREATE PROCEDURE dbo.SRP0023Detected
                              AS
                              IF {predicate}
                              BEGIN
                                  PRINT 'Rows exist';
                              END;
                              """;

        using var test = new RuleTest(
            new List<Tuple<string, string>>
            {
                Tuple.Create(TableScript, "TestTable.sql"),
                Tuple.Create(procedureScript, "SRP0023Detected.sql"),
            },
            new TSqlModelOptions(),
            SqlVersion);
        test.RunTest(UseExistenceInsteadOfCountRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(1, result.Problems.Count, $"Expected 1 problem for existence predicate '{predicate}'.");
        });
    }

    [TestMethod]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) > 1", DisplayName = "count > 1")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) >= 2", DisplayName = "count >= 2")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) = 10", DisplayName = "count = 10")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) <> 5", DisplayName = "count <> 5")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) < 3", DisplayName = "count < 3")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) <= 4", DisplayName = "count <= 4")]
    [DataRow("1 < (SELECT COUNT(*) FROM dbo.TestTable)", DisplayName = "1 < count")]
    [DataRow("(SELECT COUNT(*) FROM dbo.TestTable) + 1 > 0", DisplayName = "count arithmetic")]
    [DataRow("ABS((SELECT COUNT(*) FROM dbo.TestTable)) > 0", DisplayName = "wrapped count")]
    [DataRow("(SELECT COUNT(NullableId) FROM dbo.TestTable) > 0", DisplayName = "count nullable column")]
    public void NonExistenceComparisonNotDetected(string predicate)
    {
        var procedureScript = $"""
                              CREATE PROCEDURE dbo.SRP0023NotDetected
                              AS
                              IF {predicate}
                              BEGIN
                                  PRINT 'Rows exist';
                              END;
                              """;

        using var test = new RuleTest(
            new List<Tuple<string, string>>
            {
                Tuple.Create(TableScript, "TestTable.sql"),
                Tuple.Create(procedureScript, "SRP0023NotDetected.sql"),
            },
            new TSqlModelOptions(),
            SqlVersion);
        test.RunTest(UseExistenceInsteadOfCountRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, $"Expected 0 problems for non-existence predicate '{predicate}'.");
        });
    }
}
