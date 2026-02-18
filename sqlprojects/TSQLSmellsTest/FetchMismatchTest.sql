CREATE PROCEDURE [dbo].[TestFetchMismatch]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @id INT, @name NVARCHAR(100), @extra INT;
    DECLARE cur CURSOR FOR
        SELECT [object_id], [name] FROM [sys].[objects];
    OPEN cur;
    FETCH NEXT FROM cur INTO @id, @name, @extra;
    CLOSE cur;
    DEALLOCATE cur;
END;
