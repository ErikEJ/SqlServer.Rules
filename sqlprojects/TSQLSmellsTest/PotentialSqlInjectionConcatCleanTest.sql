CREATE PROCEDURE [dbo].[PotentialSqlInjectionConcatCleanTest]
    @param1 VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(1024);

    -- Using parameterized query properly - @sql is constructed from a constant statement
    SELECT @sql = CONCAT(N'SELECT * FROM dbo.TestTable WHERE [name] = @param1');
    EXEC [sys].[sp_executesql] @stmt = @sql, @params = N'@param1 VARCHAR(255)', @param1 = @param1;
END;
