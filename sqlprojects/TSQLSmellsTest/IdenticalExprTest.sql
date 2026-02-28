CREATE PROCEDURE [dbo].[TestIdenticalExpressions]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @x INT = 5;
    IF @x = @x
    BEGIN
        PRINT 'Always true';
    END;
END;
