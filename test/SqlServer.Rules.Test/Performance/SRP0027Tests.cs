using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Performance;

[TestClass]
public class SRP0027Tests : TestModel
{
    public SRP0027Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void ExplicitColumnConversionCastOnLeftHandSideDetected()
    {
        AssertExplicitColumnConversionProblem(
            """
            CREATE TABLE dbo.T1 (Id INT CONSTRAINT PK_T1 PRIMARY KEY);
            GO
            CREATE PROCEDURE dbo.TestProc
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT Id
                FROM dbo.T1
                WHERE CAST(Id AS NVARCHAR(10)) = '22';
            END;
            """,
            9,
            11);
    }

    [TestMethod]
    public void ExplicitColumnConversionConvertOnLeftHandSideDetected()
    {
        AssertExplicitColumnConversionProblem(
            """
            CREATE TABLE dbo.T1 (Id INT CONSTRAINT PK_T1 PRIMARY KEY);
            GO
            CREATE PROCEDURE dbo.TestProc
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT Id
                FROM dbo.T1
                WHERE CONVERT(NVARCHAR(10), Id) = '22';
            END;
            """,
            9,
            11);
    }

    [TestMethod]
    public void ExplicitColumnConversionCastOnRightHandSideDetected()
    {
        AssertExplicitColumnConversionProblem(
            """
            CREATE TABLE dbo.T1 (Id INT CONSTRAINT PK_T1 PRIMARY KEY);
            GO
            CREATE PROCEDURE dbo.TestProc
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT Id
                FROM dbo.T1
                WHERE '22' = CAST(Id AS NVARCHAR(10));
            END;
            """,
            9,
            11);
    }

    [TestMethod]
    public void ExplicitColumnConversionConvertOnRightHandSideDetected()
    {
        AssertExplicitColumnConversionProblem(
            """
            CREATE TABLE dbo.T1 (Id INT CONSTRAINT PK_T1 PRIMARY KEY);
            GO
            CREATE PROCEDURE dbo.TestProc
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT Id
                FROM dbo.T1
                WHERE '22' = CONVERT(NVARCHAR(10), Id);
            END;
            """,
            9,
            11);
    }

    private void AssertExplicitColumnConversionProblem(string sql, int line, int column)
    {
        var testFile = CreateTempSqlFile(sql);

        try
        {
            TestFiles.Add(testFile);
            ExpectedProblems.Add(new TestProblem(line, column, "SqlServer.Rules.SRP0027"));

            RunTest();
        }
        finally
        {
            if (System.IO.File.Exists(testFile))
            {
                System.IO.File.Delete(testFile);
            }
        }
    }

    private static string CreateTempSqlFile(string sql)
    {
        var filePath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"{System.Guid.NewGuid():N}.sql");

        System.IO.File.WriteAllText(
            filePath,
            sql,
            new System.Text.UTF8Encoding(true));

        return filePath;
    }
}
