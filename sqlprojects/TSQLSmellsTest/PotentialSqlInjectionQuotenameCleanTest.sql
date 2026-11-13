CREATE PROCEDURE [dbo].[PotentialSqlInjectionQuotenameCleanTest]
    @databaseName SYSNAME,
    @customerCode VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX);

    -- Identifier and literal are both wrapped in QUOTENAME, which delimits them and escapes the delimiter
    SET @sql = N'SELECT [name] FROM ' + QUOTENAME(@databaseName) + N'.[dbo].[Customers] WHERE [code] = ' + QUOTENAME(@customerCode, '''');
    EXEC [sys].[sp_executesql] @stmt = @sql;
END;
