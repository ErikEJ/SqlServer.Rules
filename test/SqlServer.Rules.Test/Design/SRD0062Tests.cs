using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0062Tests
{
    [TestMethod]
    public void AliasTypeTempTableColumnDoesNotThrow()
    {
        using var model = new TSqlModel(SqlServerVersion.Sql160, new TSqlModelOptions());
        model.AddOrUpdateObjects(
            "CREATE TYPE [dbo].[NameAlias] FROM nvarchar(128) NOT NULL;",
            "NameAlias.sql",
            new TSqlObjectOptions());

        const string sql = """
                           CREATE PROCEDURE [dbo].[AliasTypeTempTable]
                           AS
                           BEGIN
                               CREATE TABLE #Names
                               (
                                   [Name] [dbo].[NameAlias] NOT NULL
                               );
                           END;
                           """;

        AssertAnalyzeDoesNotThrow(model, sql, "AliasTypeTempTable.sql");
    }

    [TestMethod]
    public void XmlTempTableColumnDoesNotThrow()
    {
        using var model = new TSqlModel(SqlServerVersion.Sql160, new TSqlModelOptions());

        const string sql = """
                           CREATE PROCEDURE [dbo].[XmlTempTable]
                           AS
                           BEGIN
                               CREATE TABLE #Names
                               (
                                   [Name] xml NOT NULL
                               );
                           END;
                           """;

        AssertAnalyzeDoesNotThrow(model, sql, "XmlTempTable.sql");
    }

    private static void AssertAnalyzeDoesNotThrow(TSqlModel model, string sql, string fileName)
    {
        model.AddOrUpdateObjects(sql, fileName, new TSqlObjectOptions());

        var procedure = model.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Procedure).Single();
        var context = (SqlRuleExecutionContext)Activator.CreateInstance(
            typeof(SqlRuleExecutionContext),
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [model, procedure],
            culture: null)!;

        var parser = new TSql160Parser(initialQuotedIdentifiers: false);
        var fragment = parser.Parse(new StringReader(sql), out var errors);
        Assert.IsEmpty(errors);

        var scriptFragmentField = typeof(SqlRuleExecutionContext).GetField("_scriptFragment", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(scriptFragmentField);
        scriptFragmentField.SetValue(context, fragment);

        var rule = new UseProperCollationInTempTables();
        _ = rule.Analyze(context);
    }
}
