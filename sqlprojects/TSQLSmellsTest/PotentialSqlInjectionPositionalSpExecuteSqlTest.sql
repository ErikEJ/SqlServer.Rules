CREATE PROCEDURE [dbo].[PotentialSqlInjectionPositionalSpExecuteSqlTest]
    @param1 VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(1024) = N'SELECT * FROM dbo.TestTable WHERE [name] = ''' + @param1 + N'''';
    EXEC [sys].[sp_executesql] @sql;
END;

-- SRD0096
