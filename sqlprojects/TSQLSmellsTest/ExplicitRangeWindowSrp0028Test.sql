CREATE PROCEDURE dbo.ExplicitRangeWindowSrp0028Test
AS
SET NOCOUNT ON;

SELECT SUM(tt.IdCol) OVER (PARTITION BY tt.Col1
                          ORDER BY tt.Col2
                          RANGE UNBOUNDED PRECEDING) AS RollingBalance
FROM dbo.TestTableSSDT tt;
