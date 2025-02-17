CREATE PROCEDURE [dbo].[sp_Procedure5]
	@param1 int = 0,
	@param2 int
AS
	IF OBJECT_ID('tempdb..#tbl') IS NOT NULL DROP TABLE #tbl

	SELECT 'Wang' = Wang, Chung as 'Chung', Id AS [ID]
	FROM TestDatabase..Table3
	WHERE (@param1 IS NULL OR @param2 > 1)
		AND (@param2 IS NULL OR @param2 > 1)

	CREATE TABLE #tbl (
		foo int
	)

	SELECT * FROM #tbl

	EXEC sp_who2
	EXEC Procedure1 @param1 = 1, @param2 = 2
	EXEC dbo.Procedure1 1, 2

	SET NOCOUNT ON
	SET @param2 = 5;
	SELECT @param2 = 123;

	SET @param2 = 55;

	DECLARE @sql varchar(8000) = 'SELECT ' + cast(@param1 as varchar) + ' + ' + CONVERT(VARCHAR, @param2)
	EXEC( @sql)

RETURN 0
