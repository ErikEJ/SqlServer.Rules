using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Performance;
using SqlServer.Rules.Tests.Utils;

namespace SqlServer.Rules.Tests.Performance;

[TestClass]
[TestCategory("Performance")]
public class SRP0024Tests : TestCasesBase
{
    [TestMethod]
    public void CorrelatedSubqueryDetected()
    {
        const string parentTableScript = """
                                       CREATE TABLE dbo.ParentTable
                                       (
                                           ParentId INT NOT NULL
                                       );
                                       """;
        const string childTableScript = """
                                      CREATE TABLE dbo.ChildTable
                                      (
                                          ParentId INT NOT NULL
                                      );
                                      """;
        const string procedureScript = """
                                      CREATE PROCEDURE dbo.SRP0024CorrelatedDetected
                                      AS
                                      SELECT p.ParentId
                                      FROM dbo.ParentTable AS p
                                      WHERE p.ParentId = (
                                          SELECT TOP (1) c.ParentId
                                          FROM dbo.ChildTable AS c
                                          WHERE c.ParentId = p.ParentId
                                      );
                                      """;

        using var test = new RuleTest(
            new List<Tuple<string, string>>
            {
                Tuple.Create(parentTableScript, "ParentTable.sql"),
                Tuple.Create(childTableScript, "ChildTable.sql"),
                Tuple.Create(procedureScript, "SRP0024CorrelatedDetected.sql"),
            },
            new TSqlModelOptions(),
            SqlVersion);
        test.RunTest(AvoidCorrelatedSubqueriesRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(1, result.Problems.Count, "Expected 1 problem for a correlated scalar subquery against a table.");
            Assert.IsTrue(result.Problems[0].Description.Contains(AvoidCorrelatedSubqueriesRule.Message, StringComparison.Ordinal));
        });
    }

    [TestMethod]
    public void ExistsAgainstCteNotDetected()
    {
        const string parentTableScript = """
                                       CREATE TABLE dbo.ParentTable
                                       (
                                           ParentName NVARCHAR(128) NOT NULL
                                       );
                                       """;
        const string procedureScript = """
                                      CREATE PROCEDURE dbo.SRP0024ExistsAgainstCteNotDetected
                                      AS
                                      WITH set1 AS (
                                          SELECT a1 = N'a'
                                      )
                                      SELECT p.ParentName
                                      FROM dbo.ParentTable AS p
                                      WHERE NOT EXISTS (
                                          SELECT 1 / 0
                                          FROM set1
                                          WHERE a1 = p.ParentName
                                      );
                                      """;

        using var test = new RuleTest(
            new List<Tuple<string, string>>
            {
                Tuple.Create(parentTableScript, "ParentTable.sql"),
                Tuple.Create(procedureScript, "SRP0024ExistsAgainstCteNotDetected.sql"),
            },
            new TSqlModelOptions(),
            SqlVersion);
        test.RunTest(AvoidCorrelatedSubqueriesRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems for EXISTS against a CTE.");
        });
    }

    [TestMethod]
    public void ExistsAgainstTableNotDetected()
    {
        const string procedureScript = """
                                      CREATE PROCEDURE dbo.SRP0024ExistsAgainstTableNotDetected
                                      AS
                                      SELECT tt.name
                                      FROM sys.tables AS tt
                                      WHERE NOT EXISTS (
                                          SELECT 1 / 0
                                          FROM sys.indexes AS ind
                                          WHERE ind.name = tt.name
                                      );
                                      """;

        using var test = new RuleTest(
            new List<Tuple<string, string>>
            {
                Tuple.Create(procedureScript, "SRP0024ExistsAgainstTableNotDetected.sql"),
            },
            new TSqlModelOptions(),
            SqlVersion);
        test.RunTest(AvoidCorrelatedSubqueriesRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems for EXISTS against a table.");
        });
    }
}
