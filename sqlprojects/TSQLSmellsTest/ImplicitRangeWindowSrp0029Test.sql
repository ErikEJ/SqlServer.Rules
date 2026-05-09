/* Default is RANGE UNBOUNDED PRECEDING (defining the frame) */
CREATE PROCEDURE dbo.ImplicitRangeWindowSrp0029
AS
SET NOCOUNT ON;

SELECT SUM(t.Col1) OVER (PARTITION BY t.IdCol ORDER BY t.Col2) AS RollingBalance,
       ROW_NUMBER() OVER (PARTITION BY t.IdCol ORDER BY t.Col2) AS Rown
FROM dbo.TestTableSSDT AS t
ORDER BY t.Col1, t.Col2;

-- SRP0029
