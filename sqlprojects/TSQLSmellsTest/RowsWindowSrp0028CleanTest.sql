CREATE PROCEDURE dbo.RowsWindowSrp0028CleanTest
AS
SET NOCOUNT ON;

SELECT sum(tt.IdCol) OVER(PARTITION BY tt.Col1
                          ORDER BY tt.Col2
                          ROWS UNBOUNDED PRECEDING) AS RollingBalance
FROM dbo.TestTableSSDT tt;
