CREATE PROCEDURE [dbo].[TestSelectOneInExists]
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM [sys].[objects] WHERE [name] = 'test')
    BEGIN
        PRINT 'Found';
    END;
END;
