CREATE PROCEDURE [dbo].[TestNoGoto]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @i INT = 0;
    WHILE @i < 10
    BEGIN
        SET @i = @i + 1;
    END;
    RETURN @i;
END;
