CREATE Function dbo.udfTestSelectStarMultiStatementTVF()
RETURNS @RetTable TABLE(
id INTEGER
)
as
BEGIN
	SET NOCOUNT ON;
	DECLARE @s INTEGER;
	WITH cteTest
	AS
	(  
		SELECT * FROM dbo.TestTableSSDT
	)
	SELECT @s = cteTest.Col1
	FROM cteTest
	return
end

-- SML005, SML033