CREATE PROCEDURE [dbo].[TestGoto]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @i INT = 0;
    MyLabel:
    SET @i = @i + 1;
    IF @i < 10
        GOTO MyLabel;
    RETURN @i;
END;
