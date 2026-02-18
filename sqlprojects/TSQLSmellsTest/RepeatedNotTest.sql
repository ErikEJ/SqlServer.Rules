CREATE PROCEDURE [dbo].[TestRepeatedNot]
    @Active BIT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT NOT @Active = 1
    BEGIN
        PRINT 'Active';
    END;
END;
