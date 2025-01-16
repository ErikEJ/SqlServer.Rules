CREATE Function dbo.udfTestSelectStar()
RETURNS integer
as
BEGIN
	DECLARE @s INTEGER;
	WITH cteTest
	AS
	(  
		SELECT * FROM dbo.TestTableSSDT
	)
	SELECT @s = IdCol
	FROM cteTest

	RETURN @s
end

--SML005,SML033
