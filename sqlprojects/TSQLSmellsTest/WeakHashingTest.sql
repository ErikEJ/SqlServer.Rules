CREATE PROCEDURE [dbo].[TestWeakHashing]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @hash VARBINARY(8000);
    SET @hash = HASHBYTES('MD5', 'test data');

    DECLARE @hash2 VARBINARY(8000);
    SET @hash2 = HASHBYTES('SHA1', 'test data');
END;

-- SRD0074
