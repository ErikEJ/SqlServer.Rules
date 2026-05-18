CREATE PROCEDURE [dbo].[PotentialSqlInjectionConcatPropagationTest]
    @param1 VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @intermediateSql NVARCHAR(1024);
    DECLARE @sql NVARCHAR(1024);
    SET @intermediateSql = CONCAT(N'SELECT * FROM dbo.TestTable WHERE [name] = ''', @param1, N'''');
    SET @sql = @intermediateSql;
    EXEC [sys].[sp_executesql] @stmt = @sql;
END;

-- SRD0096
