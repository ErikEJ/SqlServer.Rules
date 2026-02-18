CREATE PROCEDURE [dbo].[TestStrongHashing]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @hash VARBINARY(8000);
    SET @hash = HASHBYTES('SHA2_256', 'test data');
END;
