CREATE PROCEDURE [dbo].[SingleCharVariableTest]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @x INT = 1;
    DECLARE @LongName INT = 2;
    SELECT @x, @LongName;
END;
RETURN 0;

-- SRD0079
