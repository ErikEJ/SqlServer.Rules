
CREATE PROCEDURE dbo.UnionTest
AS
SET NoCount on
SELECT * FROM dbo.TestTableSSDT
Union ALL
SELECT * FROM dbo.TestTableSSDT
UNION ALL
SELECT * FROM dbo.TestTableSSDT

--SML005
