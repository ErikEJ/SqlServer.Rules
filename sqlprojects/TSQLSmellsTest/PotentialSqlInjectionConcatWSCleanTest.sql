CREATE PROCEDURE [dbo].[PotentialSqlInjectionConcatWSCleanTest]
    @param1 VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(1024);

    -- Using parameterized query properly - no tainted variable used in dynamic SQL construction
    SELECT @sql = CONCAT_WS(N'', N'SELECT * FROM dbo.TestTable WHERE [name] = @param1');
    EXEC [sys].[sp_executesql] @stmt = @sql, @params = N'@param1 VARCHAR(255)', @param1 = @param1;
END;
