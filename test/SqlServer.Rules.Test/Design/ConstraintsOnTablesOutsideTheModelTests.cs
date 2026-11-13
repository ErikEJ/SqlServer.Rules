using System;
using System.Linq;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

/// <summary>
/// Constraints added with ALTER TABLE to a table that is not in the model, as in an ad hoc migration script.
/// </summary>
[TestClass]
public class ConstraintsOnTablesOutsideTheModelTests
{
    [TestMethod]
    [DataRow("ALTER TABLE dbo.Orders ADD PRIMARY KEY (Id);")]
    [DataRow("ALTER TABLE [dbo].[Orders] ADD [CustomerCode] VARCHAR(10) NULL, FOREIGN KEY ([CustomerCode]) REFERENCES [dbo].[Customers];")]
    public void RulesDoNotThrow(string script)
    {
        using var model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions());
        try
        {
            model.AddOrUpdateObjects(script, "script.sql", new TSqlObjectOptions());
        }
        catch (DacModelException)
        {
            // ALTER TABLE ... ADD cannot be fully modeled, but the constraint is still added and analyzed.
        }

        var result = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version).Analyze(model);

        var ruleErrors = result.GetAllErrors()
            .Select(e => e.Message)
            .Where(m => m.Contains(TestConstants.SqlServerRules, StringComparison.Ordinal))
            .ToList();
        Assert.IsEmpty(ruleErrors, string.Join(Environment.NewLine, ruleErrors));
    }
}
