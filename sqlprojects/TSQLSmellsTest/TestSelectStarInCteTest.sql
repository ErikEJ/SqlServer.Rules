CREATE PROCEDURE dbo.SelectStarInCteTest
AS 
SET nocount on ;

WITH ctex
AS (
SELECT * FROM dbo.TestTableSSDT
)
SELECT idcol,Col1 FROM ctex

go

-- SML005
