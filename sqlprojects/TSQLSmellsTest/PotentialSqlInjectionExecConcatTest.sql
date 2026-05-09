CREATE PROCEDURE [dbo].[PotentialSqlInjectionExecConcatTest]
    @param1 VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC (N'SELECT * FROM dbo.TestTable WHERE [name] = ''' + @param1 + N'''');
END;

-- SRD0096
