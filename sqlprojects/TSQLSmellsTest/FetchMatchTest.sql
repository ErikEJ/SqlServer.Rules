CREATE PROCEDURE [dbo].[TestFetchMatch]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @id INT, @name NVARCHAR(100);
    DECLARE cur CURSOR FOR
        SELECT [object_id], [name] FROM [sys].[objects];
    OPEN cur;
    FETCH NEXT FROM cur INTO @id, @name;
    CLOSE cur;
    DEALLOCATE cur;
END;
