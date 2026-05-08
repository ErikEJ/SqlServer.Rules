CREATE PROCEDURE [dbo].[PotentialSqlInjectionTest]
    @param1 VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(1024);
    SELECT @sql = N'SELECT * FROM dbo.TestTable WHERE [name] = ''' + UPPER(@param1) + N'''';
    EXEC [sys].[sp_executesql] @stmt = @sql;
END;

-- SRD0096
