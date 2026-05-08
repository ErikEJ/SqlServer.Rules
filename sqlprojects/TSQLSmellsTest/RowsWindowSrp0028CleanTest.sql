Create Procedure dbo.RowsWindowSrp0028CleanTest
as
SET NOCOUNT ON;

select sum(t.IdCol) over(partition by t.Col1
                          order by t.Col2
                          ROWS UNBOUNDED PRECEDING) as RollingBalance
from dbo.TestTableSSDT t;
