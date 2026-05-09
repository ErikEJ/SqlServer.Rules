CREATE PROCEDURE [dbo].[PotentialSqlInjectionIgnoreDeclarePropagationTest]
    @param1 VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    -- IGNORE SRD0096: Regression test to ensure ignored DECLARE initializers still propagate taint.
    DECLARE @sql NVARCHAR(1024) = N'SELECT * FROM dbo.TestTable WHERE [name] = ''' + @param1 + N'''';
    EXEC [sys].[sp_executesql] @stmt = @sql;
END;

-- SRD0096
