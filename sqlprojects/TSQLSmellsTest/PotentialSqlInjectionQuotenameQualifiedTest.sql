CREATE FUNCTION [dbo].[QUOTENAME] (@value NVARCHAR(200))
RETURNS NVARCHAR(200)
AS
BEGIN
    RETURN @value;
END;
GO

CREATE PROCEDURE [dbo].[PotentialSqlInjectionQuotenameQualifiedTest]
    @customerCode VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX);

    -- dbo.QUOTENAME is a user-defined function, not the built-in: its escaping is unknown
    SET @sql = N'SELECT [name] FROM [dbo].[Customers] WHERE [code] = ' + [dbo].[QUOTENAME](@customerCode);
    EXEC [sys].[sp_executesql] @stmt = @sql;
END;
