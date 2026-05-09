CREATE PROCEDURE [dbo].[PotentialSqlInjectionIgnorePropagationTest]
    @param1 VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(1024);
    -- IGNORE SRD0096: Regression test to ensure ignored assignments still propagate taint.
    SET @sql = N'SELECT * FROM dbo.TestTable WHERE [name] = ''' + @param1 + N'''';
    EXEC [sys].[sp_executesql] @stmt = @sql;
END;

-- SRD0096
