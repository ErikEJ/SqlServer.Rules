
CREATE PROCEDURE dbo.SelectStarOutOfCteTest
AS 
Set nocount on;

WITH ctex
AS (
SELECT * FROM dbo.TestTableSSDT
)
SELECT * FROM ctex

go