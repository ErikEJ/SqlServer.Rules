CREATE PROCEDURE [dbo].[PotentialSqlInjectionConcatCleanTest]
    @param1 VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @unused NVARCHAR(255);
    DECLARE @sql NVARCHAR(1024);

    -- @param1 is used in CONCAT but not for dynamic SQL statement construction
    SELECT @unused = CONCAT(@param1, N'');

    -- Using parameterized query properly - no tainted variable used in dynamic SQL construction
    SELECT @sql = CONCAT(N'SELECT * FROM dbo.TestTable WHERE [name] = @param1');
    EXEC [sys].[sp_executesql] @stmt = @sql, @params = N'@param1 VARCHAR(255)', @param1 = @param1;
END;
