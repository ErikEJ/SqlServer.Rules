CREATE VIEW [View1]
	AS 
	SELECT Table1.* 
	FROM Table1 WITH (nolock)
	INNER JOIN dbo.Table2 t2 
	ON 1 = 1 
		AND dbo.Table1.[Table1Id] = t2.[Tbl1Id]
		AND dbo.Table1.[Table1Id] = t2.[Tbl1Id]
		AND GETDATE() > GETDATE() - 1 
		AND LEFT(dbo.Table1.[Table1Id], 20) = LEFT(t2.[Tbl1Id], 20)
