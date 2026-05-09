CREATE PROCEDURE dbo.ImplicitRangeWindowSrp0029IgnoredFunctions
AS
SET NOCOUNT ON;

SELECT ROW_NUMBER() OVER (PARTITION BY tt.IdCol ORDER BY tt.Col2) AS RowNum,
       CUME_DIST() OVER (PARTITION BY tt.IdCol ORDER BY tt.Col2) AS CumeDistVal,
       PERCENT_RANK() OVER (PARTITION BY tt.IdCol ORDER BY tt.Col2) AS PercentRankVal
FROM dbo.TestTableSSDT AS tt;
