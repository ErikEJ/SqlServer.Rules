CREATE PROCEDURE [dbo].[PotentialSqlInjectionQuotenameMixedTest]
    @databaseName SYSNAME,
    @customerCode VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX);

    -- The database name is quoted, the customer code is still concatenated as is
    SET @sql = N'SELECT [name] FROM ' + QUOTENAME(@databaseName) + N'.[dbo].[Customers] WHERE [code] = ''' + @customerCode + N'''';
    EXEC [sys].[sp_executesql] @stmt = @sql;
END;
