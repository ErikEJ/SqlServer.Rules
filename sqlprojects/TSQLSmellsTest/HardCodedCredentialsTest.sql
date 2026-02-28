CREATE PROCEDURE [dbo].[TestHardCodedCredentials]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Password NVARCHAR(100) = 'MySecret123';
    DECLARE @ApiKey NVARCHAR(100);
    SET @ApiKey = 'sk-1234567890';
END;

-- SRD0075
