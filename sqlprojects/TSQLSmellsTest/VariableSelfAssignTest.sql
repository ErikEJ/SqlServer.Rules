CREATE PROCEDURE [dbo].[TestVariableSelfAssign]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @x INT = 5;
    SET @x = @x;
END;

-- SRD0072
