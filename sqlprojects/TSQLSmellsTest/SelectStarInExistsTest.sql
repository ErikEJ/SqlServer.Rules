CREATE PROCEDURE [dbo].[TestSelectStarInExists]
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT * FROM [sys].[objects] WHERE [name] = 'test')
    BEGIN
        PRINT 'Found';
    END;
END;

-- SRP0025
