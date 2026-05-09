/* Default is RANGE UNBOUNDED PRECEDING (defining the frame) */
CREATE PROCEDURE dbo.ImplicitRangeWindowSrp0029
AS
SET NOCOUNT ON;

SELECT SUM(t.Col1) OVER (PARTITION BY tt.IdCol ORDER BY tt.Col2) AS RollingBalance,
       ROW_NUMBER() OVER (PARTITION BY tt.IdCol ORDER BY tt.Col2) AS Rown
FROM dbo.TestTableSSDT AS tt
ORDER BY tt.Col1, tt.Col2;

-- SRP0029
