CREATE PROCEDURE [dbo].[Procedure1] 
	@param1 int = 0,
	@param2 float -- IGNORE SRD0016 
WITH RECOMPILE 
AS
	SET ROWCOUNT 500
	-- IGNORE SRP0003
	SELECT SUM(DISTINCT [Table1Id]) 
	FROM Table1 
	WHERE UPPER(FirstName) = 'FOO' AND LastName = ISNULL(@param1, LastName) AND Age = CAST(@param1 AS INT)

	WAITFOR DELAY '00:00:10'

	DECLARE test_cursor CURSOR
	READ_ONLY
	FOR SELECT CAST(FirstName AS VARCHAR) + ' ' + LastName FROM Table1 order by 1 desc

	DECLARE @name varchar(1000)
	OPEN test_cursor

	FETCH NEXT FROM test_cursor INTO @name
	WHILE (@@fetch_status <> -1)
	BEGIN
		IF (@@fetch_status <> -2)
		BEGIN
			DECLARE @message varchar(100)
			SELECT @message = 'my name is: ' + @name
			PRINT @message
		END
		FETCH NEXT FROM test_cursor INTO @name
	END

RETURN 0
GRANT EXEC ON Procedure1 TO PUBLIC
