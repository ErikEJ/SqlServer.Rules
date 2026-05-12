using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Dac.Visitors;

namespace SqlServer.Rules.Tests.Utils;

[TestClass]
public class TypesVisitorTests
{
    [TestMethod]
    public void ConstructorAllowsEmptyTypes()
    {
        var visitor = new TypesVisitor();

        Assert.AreEqual(0, visitor.Count);
        Assert.AreEqual(0, visitor.Statements.Count);
    }

    [TestMethod]
    public void ConstructorAllowsNullTypes()
    {
        var visitor = new TypesVisitor((Type[])null);

        Assert.AreEqual(0, visitor.Count);
        Assert.AreEqual(0, visitor.Statements.Count);
    }

    [TestMethod]
    public void VisitWithNoTypesIsANoOp()
    {
        var visitor = new TypesVisitor();

        visitor.Visit(new SelectStatement());

        Assert.AreEqual(0, visitor.Count);
        Assert.AreEqual(0, visitor.Statements.Count);
    }
}
