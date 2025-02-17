
CREATE PROCEDURE dbo.SelectStarOutOfCteTest1
AS 
Set NoCount on;

WITH ctex
AS (
SELECT * FROM dbo.TestTable
)
SELECT * FROM ctex

go

-- SML005
